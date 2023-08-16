using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using S16.Collections;
using CsvEditor.Wildcard;
using CsvEditor.Controls;

namespace CsvEditor.ViewModels
{
    [Flags]
    public enum FindOptions : uint 
    {
        None = 0,
        MatchWholeWord = 1 << 0,
        MatchCase = 1 << 1,
        UseRegex = 1 << 2,
        InSelections = 1 << 3,
        PreserveCase = 1 << 4,
        Backwards = 1 << 5,
        Replace = 1 << 6,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextPointer
    {
        public int Offset;
        public string Text;

        public int Length
        {
            get
            {
                if (string.IsNullOrEmpty(Text)) return 0;
                return Text.Length;
            }
        }

        public TextPointer(string text)
        {
            Offset = 0;
            Text = text;
        }

        public TextPointer(int offset, string text)
        {
            Offset = offset;
            Text = text;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FoundResult : ICloneable
    {
        public int Index;
        public int Row;
        public int Column;
        public TextPointer[] Pointers;

        public FoundResult(int index)
        {
            Index = index;
            Row = -1;
            Column = -1;
            Pointers = new TextPointer[0];
        }

        public FoundResult(int index, int row, int col, int count)
        {
            Index = index;
            Row = row;
            Column = col;
            Pointers = new TextPointer[count];
        }
        public FoundResult(int index, int row, int col, params TextPointer[] pointers)
        {
            Index = index;
            Row = row;
            Column = col;
            Pointers = new TextPointer[pointers.Length];
            Array.Copy(pointers, Pointers, pointers.Length);
        }

        public FoundResult Clone()
            => new FoundResult(Index, Row, Column, Pointers);

        object ICloneable.Clone()
            => Clone();
    }

    public class FindAndReplace
    {
        #region Variables
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly MainViewModel model;
        private readonly FindAndReplaceBar ctrl;
        private readonly SynchronizationContext syncContext;
        private readonly List<FoundResult> results;
        private Regex findRegex = null;
        private int findIndex = 0;
        private int subIndex = 0;
        private int textStartAt = 0;
        #endregion

        #region Constructors
        internal FindAndReplace(MainViewModel model, FindAndReplaceBar ctrl)
        {
            this.model = model;
            this.ctrl = ctrl;

            syncContext = SynchronizationContext.Current;
            results = new List<FoundResult>();

            model.PropertyChanged += Model_PropertyChanged;

            ctrl.HasSelection = model.HasData;
            ctrl.FindTextChanged += Ctrl_FindTextChanged;
            ctrl.FindAccepted += Ctrl_FindAccepted;
            ctrl.ReplaceAccepted += Ctrl_ReplaceAccepted;
        }
        #endregion

        #region Methods
        public static Regex FindTextToRegex(string criteria, FindOptions options)
        {
            try
            {
                string pattern = criteria;

                var regexOptions = RegexOptions.Multiline;

                if (!options.HasFlag(FindOptions.MatchCase))
                    regexOptions |= RegexOptions.IgnoreCase;

                if (options.HasFlag(FindOptions.Backwards))
                    regexOptions |= RegexOptions.RightToLeft;

                Regex regex;
                if (options.HasFlag(FindOptions.UseRegex))
                {
                    if (options.HasFlag(FindOptions.MatchWholeWord))
                        pattern = @"\b" + pattern + @"\b";

                    regex = new Regex(pattern, regexOptions);
                }
                else
                {
                    WildcardOptions wcopt = WildcardOptions.None;
                    if (!options.HasFlag(FindOptions.MatchCase))
                        wcopt = WildcardOptions.IgnoreCase;

                    var wildcardPattern = new WildcardPattern(pattern, wcopt);

                    WildcardPatternToRegexParser parser = new WildcardPatternToRegexParser();
                    WildcardPatternParser.Parse(wildcardPattern, parser);

                    pattern = parser.RegexPattern.TrimStart('^').TrimEnd('$');

                    if (options.HasFlag(FindOptions.MatchWholeWord))
                        pattern = @"\b" + pattern + @"\b";

                    regex = new Regex(pattern, regexOptions | RegexOptions.Compiled);
                }

                return regex;
            }
            catch (Exception)
            { }

            return null;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasHeader" && ctrl.CanFind(SearchDirection.All))
            {
                FindAll(ctrl.FindText, GetFindOptions());
            }
        }

        private void Ctrl_FindTextChanged(object sender, RoutedFindEventArgs e)
        {
            FindOptions options = GetFindOptions(e);
            findIndex = -1;
            FindAll(e.SearchText, options);
        }

        private void Ctrl_FindAccepted(object sender, RoutedFindEventArgs e)
        {
            FindOptions options = GetFindOptions(e);

            if (e.SearchDirection == SearchDirection.Previous)
                FindPrevious(options);
            else if (e.SearchDirection == SearchDirection.Next)
                FindNext(options);
        }

        private void Ctrl_ReplaceAccepted(object sender, RoutedReplaceEventArgs e)
        {
            FindOptions options = GetReplaceOptions(e);

            if (e.ReplaceAll)
                ReplaceAll(e.SearchText, e.ReplaceText, options);
            else
                Replace(e.SearchText, e.ReplaceText, options);
        }

        private FindOptions GetFindOptions()
        {
            FindOptions options = FindOptions.None;

            if (ctrl.MatchWholeWord) options |= FindOptions.MatchWholeWord;
            if (ctrl.MatchCase) options |= FindOptions.MatchCase;
            if (ctrl.UseRegex) options |= FindOptions.UseRegex;
            if (ctrl.InSelection) options |= FindOptions.InSelections;
            if (ctrl.PreserveCase) options |= FindOptions.PreserveCase;
            if (ctrl.IsReplace) options |= FindOptions.Replace;

            return options;
        }

        private FindOptions GetFindOptions(RoutedFindEventArgs e)
        {
            FindOptions options = FindOptions.None;

            if (e.MatchWholeWord) options |= FindOptions.MatchWholeWord;
            if (e.MatchCase) options |= FindOptions.MatchCase;
            if (e.UseRegex) options |= FindOptions.UseRegex;
            if (e.InSelection) options |= FindOptions.InSelections;
            if (ctrl.PreserveCase) options |= FindOptions.PreserveCase;
            if (ctrl.IsReplace) options |= FindOptions.Replace;

            return options;
        }

        private FindOptions GetReplaceOptions(RoutedReplaceEventArgs e)
        {
            FindOptions options = FindOptions.None;

            if (e.MatchWholeWord) options |= FindOptions.MatchWholeWord;
            if (e.MatchCase) options |= FindOptions.MatchCase;
            if (e.UseRegex) options |= FindOptions.UseRegex;
            if (e.InSelection) options |= FindOptions.InSelections;
            if (e.PreserveCase) options |= FindOptions.PreserveCase;
            options |= FindOptions.Replace;

            return options;
        }

        public async void FindAll(string criteria, FindOptions options)
        {
            await _semaphoreSlim.WaitAsync();

            FoundResult lastRes = new FoundResult(-1);
            if (findIndex > -1 && findIndex < results.Count)
            {
                var current = results[findIndex];
                lastRes = current.Clone();
            }

            results.Clear();
            findIndex = -1;
            findRegex = null;

            if (!string.IsNullOrWhiteSpace(criteria))
            {
                List2D<string> list = model.ItemsSource;
                findRegex = FindTextToRegex(criteria, options);

                list.ForAllColumn((val, y, x) =>
                {
                    if (model.HasHeader && y == 0) return;

                    var matches = findRegex.Matches(val);
                    if (matches.Count > 0)
                    {
                        if (lastRes.Index > -1 && findIndex == -1)
                        {
                            if (lastRes.Row == y && lastRes.Column == x)
                                findIndex = results.Count;
                            else if (lastRes.Row == y && lastRes.Column < x)
                                findIndex = results.Count;
                            else if (lastRes.Row < y)
                                findIndex = results.Count;
                        }

                        var res = new FoundResult(results.Count, y, x, matches.Count);
                        for (int i = 0; i < matches.Count; i++)
                        {
                            var m = matches[i];
                            res.Pointers[i] = new TextPointer(m.Index, m.Value);
                        }
                        results.Add(res);
                    }
                });
            }

            await Task.Delay(10);
            _semaphoreSlim.Release();

            syncContext.Post(args => 
            {
                ctrl.FoundCount = results.Count;
                ctrl.FindIndex = findIndex + 1;

            }, this);
        }

        public void FindPrevious(FindOptions options)
        {
            if (results.Count == 0) return;

            findIndex = Math.Max(0, findIndex - 1);
            subIndex = 0;
            textStartAt = 0;

            var selection = options.HasFlag(FindOptions.InSelections);
            var hasHeader = model.HasHeader;
            var current = results[findIndex];

            if (hasHeader && current.Row == 0)
            {
                ctrl.FindIndex = findIndex;
                return;
            }

            if (selection && current.Pointers != null && current.Pointers.Length > 0)
            {
                model.SelectCell(current.Row, current.Column, true, current.Pointers[0].Offset, current.Pointers[0].Length);
            }
            else
            {
                model.SelectCell(current.Row, current.Column, selection);
            }
            ctrl.FindIndex = findIndex + 1;
        }

        public void FindNext(FindOptions options)
        {
            if (results.Count == 0) return;

            findIndex = Math.Min(results.Count - 1, findIndex + 1);
            subIndex = 0;
            textStartAt = 0;

            var selection = options.HasFlag(FindOptions.InSelections);
            var hasHeader = model.HasHeader;
            var current = results[findIndex];
            
            if (hasHeader && current.Row == 0)
            {
                findIndex += 1;
                current = results[findIndex];
            }

            if (selection && current.Pointers != null && current.Pointers.Length > 0)
            {
                model.SelectCell(current.Row, current.Column, true, current.Pointers[0].Offset, current.Pointers[0].Length);
            }
            else
            {
                model.SelectCell(current.Row, current.Column, selection);
            }
            ctrl.FindIndex = findIndex + 1;
        }

        public void Replace(string criteria, string replaceText, FindOptions options)
        {
            if (findRegex == null || results.Count == 0) return;

            if (!string.IsNullOrWhiteSpace(replaceText))
            {
                List2D<string> list = model.ItemsSource;

                var selection = options.HasFlag(FindOptions.InSelections);
                var preserveCase = options.HasFlag(FindOptions.PreserveCase);
                var hasHeader = model.HasHeader;

                var current = results[Math.Max(0, findIndex)];
                
                if (findIndex == -1 || subIndex == current.Pointers.Length)
                {
                    subIndex = 0;
                    textStartAt = 0;
                    findIndex = Math.Min(results.Count - 1, findIndex + 1);
                    current = results[findIndex];
                }
                subIndex++;

                if (hasHeader && current.Row == 0)
                {
                    findIndex += 1;
                    current = results[findIndex];
                }

                var val = list[current.Row, current.Column];

                list[current.Row, current.Column] = findRegex.Replace(val, match =>
                {
                    var result = match.Result(replaceText);
                    if (preserveCase)
                    {
                        var oldVal = match.Value;
                        var caseType = Cases.Of(oldVal);
                        result = Cases.ToCase(result, caseType);
                    }

                    textStartAt = match.Index + result.Length;
                    return result;
                }, 1, textStartAt);

                model.UpdateGrid(false);
                model.MarkEdited(true);

                if (selection && current.Pointers != null && current.Pointers.Length > 0)
                {
                    model.SelectCell(current.Row, current.Column, true, current.Pointers[0].Offset, current.Pointers[0].Length);
                }
                else
                {
                    model.SelectCell(current.Row, current.Column, selection);
                }

                ctrl.FindIndex = findIndex + 1;

                if (findIndex == results.Count - 1)
                {
                    findIndex = -1;
                    FindAll(criteria, options);
                }
            }
        }

        public void ReplaceAll(string criteria, string replaceText, FindOptions options)
        {
            if (findRegex == null || results.Count == 0) return;

            if (!string.IsNullOrWhiteSpace(replaceText))
            {
                List2D<string> list = model.ItemsSource;
                var preserveCase = options.HasFlag(FindOptions.PreserveCase);

                foreach (var res in results)
                {
                    var val = list[res.Row, res.Column];
                    
                    list[res.Row, res.Column] = findRegex.Replace(val, match =>
                    {
                        var result = match.Result(replaceText);
                        if (preserveCase)
                        {
                            var oldVal = match.Value;
                            var caseType = Cases.Of(oldVal);
                            return Cases.ToCase(result, caseType);
                        }
                        return result;
                    });
                }

                model.UpdateGrid(false);
                model.MarkEdited(true);

                findIndex = -1;
                FindAll(criteria, options);
            }
        }
        #endregion
    }

    public partial class MainViewModel
    {
        public FindAndReplace CreateFindAndReplace(FindAndReplaceBar ctrl)
        {
            return new FindAndReplace(this, ctrl);
        }
    }
}
