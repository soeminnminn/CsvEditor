// https://github.com/autumn009/NestedHtmlWriter
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NestedHtmlWriter
{
    /// <summary>
    /// Main name space of NestedHtmlWriter
    /// </summary>
    /// <remarks>This is a namespace for NestedHtmlWriter</remarks>
    [System.Runtime.CompilerServices.CompilerGenerated()]
    class NamespaceDoc
    {
    }
    /// <summary>
    /// Type of document types
    /// </summary>
    public enum NhDocumentType
    {
        /// <summary>
        /// XHTML 1.1
        /// </summary>
        Xhtml11,
        /// <summary>
        /// XHTML 1.0 Strict
        /// </summary>
        Xhtml10Strict,
        /// <summary>
        /// XHTML 1.0 Transitional
        /// </summary>
        Xhtml10Transitional,
        /// <summary>
        /// HTML5
        /// </summary>
        Html5,
    }

    /// <summary>
    /// Global settings
    /// </summary>
    public class NhSetting
    {
        /// <summary>
        /// Require strick cheking
        /// </summary>
        /// <remarks>
        /// If you check strick, many invalid operation may raise exceotion.
        /// But need for speed to select false
        /// </remarks>
        public static bool StrictNestChecking = true;
    }
    /// <summary>
    /// Invalid operations
    /// </summary>
    public class NhException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NhException()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Exception message</param>
        public NhException(string msg)
            : base(msg)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Exception message</param>
        /// <param name="ex">excetion object</param>
        public NhException(string msg, Exception ex)
            : base(msg, ex)
        {
        }
    }
    /// <summary>
    /// Base class of all elements
    /// </summary>
    public class NhBase
    {
        /// <summary>
        /// writer for output
        /// </summary>
        protected TextWriter writer;
        /// <summary>
        /// parent object
        /// </summary>
        protected NhBase parent;
        /// <summary>
        /// check if locked
        /// </summary>
        /// <remarks>
        /// don't call this if not strick checking mode
        /// </remarks>
        protected void checkLock()
        {
            if (lockFlag)
            {
                throw new NhException("NestedHtmlWriter item was locked");
            }
        }
        /// <summary>
        /// Constructtor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent object</param>
        public NhBase(TextWriter writer, NhBase parent)
        {
            this.writer = writer;
            this.parent = parent;
        }
        /// <summary>
        /// Output without any conversion.
        /// Don't use this method until you need.
        /// </summary>
        /// <param name="rawString">raw text</param>
        public virtual void WriteRawString(string rawString)
        {
            writer.Write(rawString);
        }
        /// <summary>
        /// output any attributes
        /// </summary>
        /// <param name="name">attribute name, if you have namespace prefix, PRIFIX:NAME style name is required.</param>
        /// <param name="val"> attribute value</param>
        public void WriteAttribute(string name, string val)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            writer.Write(' ');
            writer.Write(name);
            writer.Write("=\"");
            writer.Write(NhUtil.QuoteText(val));
            writer.Write("\"");
        }
        /// <summary>
        /// output class attribute
        /// </summary>
        /// <param name="val"> attribute value</param>
        public void WriteClassAttr(string val)
        {
            WriteAttribute("class", val);
        }
        /// <summary>
        /// output id attribute
        /// </summary>
        /// <param name="val"> attribute value</param>
        public void WriteIDAttr(string val)
        {
            WriteAttribute("id", val);
        }
        private bool lockFlag = false;
        internal void Lock()
        {
            if (lockFlag)
            {
                throw new NhException("Try to lock item already locked");
            }
            lockFlag = true;
        }
        internal void Unlock()
        {
            if (!lockFlag)
            {
                throw new NhException("Try to unlock item not locked");
            }
            lockFlag = false;
        }
    }
    /// <summary>
    /// a generic class for empty element, use by derive.
    /// </summary>
    public class NhEmpty : NhBase, IDisposable
    {
        private string tagName;
        /// <summary>
        /// NhEmpty class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="tagName">tag name, if you use namespace prifx, use PREFIX:NAME style.</param>
        /// <param name="parent">parent element object</param>
        public NhEmpty(TextWriter writer, string tagName, NhBase parent)
            : base(writer, parent)
        {
            if (NhSetting.StrictNestChecking && parent != null) parent.Lock();
            this.tagName = tagName;
            writer.Write('<');
            writer.Write(tagName);
        }
        /// <summary>
        /// Release allocated resources
        /// </summary>
        public void Dispose()
        {
            writer.Write(" />");
            if (!(parent is NhTextAvailable))
            {
                writer.Write("\r\n");
            }
            if (NhSetting.StrictNestChecking && parent != null) parent.Unlock();
        }
    }
    /// <summary>
    /// a generic class for not empty element, use by derive.
    /// </summary>
    public class NhNotEmpty : NhBase, IDisposable
    {
        private string tagName;
        private bool startTagClosed = false;
        /// <summary>
        /// confirm tag is closed and if closed, outout '&gt;'
        /// </summary>
        protected void confirmStartTagClose()
        {
            if (!startTagClosed)
            {
                writer.Write('>');
                if (!(this is NhTextAvailable))
                {
                    writer.Write("\r\n");
                }
                startTagClosed = true;
            }
        }

        /// <summary>
        /// output any text without any conmversion
        /// </summary>
        /// <param name="rawString">text of XML fragment</param>
        public override void WriteRawString(string rawString)
        {
            confirmStartTagClose();
            base.WriteRawString(rawString);
        }
        /// <summary>
        /// NhNotEmpty class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="tagName">tag name, if you use namespace prifx, use PREFIX:NAME style.</param>
        /// <param name="parent">parent element object</param>
        public NhNotEmpty(TextWriter writer, string tagName, NhBase parent)
            : base(writer, parent)
        {
            if (NhSetting.StrictNestChecking && parent != null) parent.Lock();
            this.tagName = tagName;
            writer.Write('<');
            writer.Write(tagName);
        }
        /// <summary>
        /// Release allocated resources
        /// </summary>
        public void Dispose()
        {
            confirmStartTagClose();
            writer.Write("</");
            writer.Write(tagName);
            writer.Write('>');
            if (!(parent is NhTextAvailable))
            {
                writer.Write("\r\n");
            }
            if (NhSetting.StrictNestChecking && parent != null) parent.Unlock();
        }
    }
    /// <summary>
    /// element class for content has text. use by Derive
    /// </summary>
    public class NhTextAvailable : NhNotEmpty
    {
        /// <summary>
        /// NhTextAvailable class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="tagName">tag name, if you use namespace prifx, use PREFIX:NAME style.</param>
        /// <param name="parent">parent element object</param>
        public NhTextAvailable(TextWriter writer, string tagName, NhBase parent)
            : base(writer, tagName, parent)
        {
        }
        /// <summary>
        /// output text
        /// </summary>
        /// <param name="text">text for output</param>
        public void WriteText(string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write(NhUtil.QuoteText(text));
        }
    }
    /// <summary>
    /// element class for content is inlinet. use by Derive
    /// </summary>
    public class NhInline : NhTextAvailable
    {
        /// <summary>
        /// NhInline class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="tagName">tag name, if you use namespace prifx, use PREFIX:NAME style.</param>
        /// <param name="parent">parent element object</param>
        public NhInline(TextWriter writer, string tagName, NhBase parent)
            : base(writer, tagName, parent)
        {
        }
        /// <summary>
        /// strong elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhStrong CreateStrong()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhStrong(writer, this);
        }
        /// <summary>
        /// em elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhEm CreateEm()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhEm(writer, this);
        }
        /// <summary>
        /// span elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhSpan CreateSpan()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhSpan(writer, this);
        }
        /// <summary>
        /// q elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhQ CreateQ()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhQ(writer, this);
        }
        /// <summary>
        /// a elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhA CreateA()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhA(writer, this);
        }
        /// <summary>
        /// img elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhImg CreateImg()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhImg(writer, this);
        }
        /// <summary>
        /// br elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhBr CreateBr()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhBr(writer, this);
        }
        /// <summary>
        /// input elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhInput CreateInput()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhInput(writer, this);
        }
        /// <summary>
        /// label elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhLabel CreateLabel()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLabel(writer, this);
        }
        /// <summary>
        /// select elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhSelect CreateSelect()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhSelect(writer, this);
        }
        /// <summary>
        /// textarea elemnt object creation
        /// </summary>
        /// <param name="cols">cols attribute value</param>
        /// <param name="rows">rows attribute value</param>
        /// <returns>created object</returns>
        public NhTextArea CreateTextArea(int cols, int rows)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhTextArea(writer, this, cols, rows);
        }
        /// <summary>
        /// output a element which has href attribute and text content only
        /// </summary>
        /// <param name="href">href attribute value</param>
        /// <param name="text">text for content</param>
        public void WriteAText(string href, string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<a href=\"");
            writer.Write(NhUtil.QuoteText(href));
            writer.Write("\">");
            writer.Write(NhUtil.QuoteText(text));
            writer.Write("</a>");
        }
        /// <summary>
        /// output img element which has src, alt, width, heigh attribute
        /// </summary>
        /// <param name="src">src attribute value</param>
        /// <param name="alt">alt attribute value</param>
        /// <param name="width">width attribute value</param>
        /// <param name="height">height attribute value</param>
        public void WriteImg(string src, string alt, int width, int height)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<img src=\"");
            writer.Write(NhUtil.QuoteText(src));
            writer.Write("\" alt=\"");
            writer.Write(NhUtil.QuoteText(alt));
            writer.Write("\" width=\"");
            writer.Write(width.ToString());
            writer.Write("\" height=\"");
            writer.Write(height.ToString());
            writer.Write("\" />");
        }
        /// <summary>
        /// output img element which has src, alt attribute
        /// </summary>
        /// <param name="src">src attribute value</param>
        /// <param name="alt">alt attribute value</param>
        public void WriteImg(string src, string alt)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<img src=\"");
            writer.Write(NhUtil.QuoteText(src));
            writer.Write("\" alt=\"");
            writer.Write(NhUtil.QuoteText(alt));
            writer.Write("\" />");
        }
        /// <summary>
        /// output br element without attribute
        /// </summary>
        public void WriteBr()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<br />");
        }
    }
    /// <summary>
    /// img elemnt output class
    /// </summary>
    public class NhImg : NhEmpty
    {
        /// <summary>
        /// NhImg class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhImg(TextWriter writer, NhBase parent)
            : base(writer, "img", parent)
        {
        }
    }
    /// <summary>
    /// br elemnt output class
    /// </summary>
    public class NhBr : NhEmpty
    {
        /// <summary>
        /// NhBr class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhBr(TextWriter writer, NhBase parent)
            : base(writer, "br", parent)
        {
        }
    }
    /// <summary>
    /// span elemnt output class
    /// </summary>
    public class NhSpan : NhInline
    {
        /// <summary>
        /// NhSpan class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhSpan(TextWriter writer, NhBase parent)
            : base(writer, "span", parent)
        {
        }
    }
    /// <summary>
    /// q elemnt output class
    /// </summary>
    public class NhQ : NhInline
    {
        /// <summary>
        /// NhQ class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhQ(TextWriter writer, NhBase parent)
            : base(writer, "q", parent)
        {
        }
    }
    /// <summary>
    /// strong elemnt output class
    /// </summary>
    public class NhStrong : NhInline
    {
        /// <summary>
        /// NhStrong class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhStrong(TextWriter writer, NhBase parent)
            : base(writer, "strong", parent)
        {
        }
    }
    /// <summary>
    /// em elemnt output class
    /// </summary>
    public class NhEm : NhInline
    {
        /// <summary>
        /// NhEm class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhEm(TextWriter writer, NhBase parent)
            : base(writer, "em", parent)
        {
        }
    }
    /// <summary>
    /// a elemnt output class
    /// </summary>
    public class NhA : NhInline
    {
        /// <summary>
        /// NhA class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhA(TextWriter writer, NhBase parent)
            : base(writer, "a", parent)
        {
        }
    }
    /// <summary>
    /// p elemnt output class
    /// </summary>
    public class NhP : NhInline
    {
        /// <summary>
        /// NhP class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhP(TextWriter writer, NhBase parent)
            : base(writer, "p", parent)
        {
        }
    }
    /// <summary>
    /// caption elemnt output class
    /// </summary>
    public class NhCaption : NhInline
    {
        /// <summary>
        /// NhCaption class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhCaption(TextWriter writer, NhBase parent)
            : base(writer, "caption", parent)
        {
        }
    }
    /// <summary>
    /// inline content li elemnt output class
    /// </summary>
    [System.Obsolete("NhLi was replaces NhLiInline")]
    public class NhLi : NhInline
    {
        /// <summary>
        /// NhLi class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhLi(TextWriter writer, NhBase parent)
            : base(writer, "li", parent)
        {
        }
    }
    /// <summary>
    /// inline content li elemnt output class
    /// </summary>
    public class NhLiInline : NhInline
    {
        /// <summary>
        /// NhLiInline class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhLiInline(TextWriter writer, NhBase parent)
            : base(writer, "li", parent)
        {
        }
    }
    /// <summary>
    /// block content li elemnt output class
    /// </summary>
    public class NhLiBlock : NhBlock
    {
        /// <summary>
        /// NhLiBlock class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhLiBlock(TextWriter writer, NhBase parent)
            : base(writer, "li", parent)
        {
        }
    }
    /// <summary>
    /// inline content td elemnt output class
    /// </summary>
    public class NhTdInline : NhInline
    {
        /// <summary>
        /// NhTdInline class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhTdInline(TextWriter writer, NhBase parent)
            : base(writer, "td", parent)
        {
        }
    }
    /// <summary>
    /// block content td elemnt output class
    /// </summary>
    public class NhTdBlock : NhBlock
    {
        /// <summary>
        /// NhTdBlock class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhTdBlock(TextWriter writer, NhBase parent)
            : base(writer, "td", parent)
        {
        }
    }
    /// <summary>
    /// inline content th elemnt output class
    /// </summary>
    public class NhThInline : NhInline
    {
        /// <summary>
        /// NhThInline class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhThInline(TextWriter writer, NhBase parent)
            : base(writer, "th", parent)
        {
        }
    }
    /// <summary>
    /// block content th elemnt output class
    /// </summary>
    public class NhThBlock : NhBlock
    {
        /// <summary>
        /// NhThBlock class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhThBlock(TextWriter writer, NhBase parent)
            : base(writer, "th", parent)
        {
        }
    }
    /// <summary>
    /// tr elemnt output class
    /// </summary>
    public class NhTr : NhNotEmpty
    {
        /// <summary>
        /// NhTr class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhTr(TextWriter writer, NhBase parent)
            : base(writer, "tr", parent)
        {
        }
        /// <summary>
        /// inline content td elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhTdInline CreateTdInline()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhTdInline(writer, this);
        }
        /// <summary>
        /// block content td elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhTdBlock CreateTdBlock()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhTdBlock(writer, this);
        }
        /// <summary>
        /// inline content th elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhThInline CreateThInline()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhThInline(writer, this);
        }
        /// <summary>
        /// block content th elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhThBlock CreateThBlock()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhThBlock(writer, this);
        }
    }
    /// <summary>
    /// ul elemnt output class
    /// </summary>
    public class NhUl : NhNotEmpty
    {
        /// <summary>
        /// NhUl class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhUl(TextWriter writer, NhBase parent)
            : base(writer, "ul", parent)
        {
        }
        /// <summary>
        /// li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        [System.Obsolete()]
        public NhLi CreateLi()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLi(writer, this);
        }

        /// <summary>
        /// inline content li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhLiInline CreateLiInline()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLiInline(writer, this);
        }

        /// <summary>
        /// block content li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhLiBlock CreateLiBlock()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLiBlock(writer, this);
        }

        /// <summary>
        /// output li element which has only text content without any attribute
        /// </summary>
        /// <param name="text">text for content</param>
        public void WriteLiText(string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<li>");
            writer.Write(NhUtil.QuoteText(text));
            writer.Write("</li>\r\n");
        }
    }
    /// <summary>
    /// ol elemnt output class
    /// </summary>
    public class NhOl : NhNotEmpty
    {
        /// <summary>
        /// NhOl class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhOl(TextWriter writer, NhBase parent)
            : base(writer, "ol", parent)
        {
        }
        /// <summary>
        /// inline content li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        [System.Obsolete()]
        public NhLi CreateLi()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLi(writer, this);
        }

        /// <summary>
        /// inline content li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhLiInline CreateLiInline()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLiInline(writer, this);
        }

        /// <summary>
        /// block content li elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhLiBlock CreateLiBlock()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhLiBlock(writer, this);
        }

        /// <summary>
        /// output li element which has only text content without any attribute
        /// </summary>
        /// <param name="text">text for content</param>
        public void WriteLiText(string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<li>");
            writer.Write(NhUtil.QuoteText(text));
            writer.Write("</li>\r\n");
        }
    }
    /// <summary>
    /// table elemnt output class
    /// </summary>
    public class NhTable : NhNotEmpty
    {
        /// <summary>
        /// NhTable class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhTable(TextWriter writer, NhBase parent)
            : base(writer, "table", parent)
        {
        }
        /// <summary>
        /// tr elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhTr CreateTr()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhTr(writer, this);
        }
        /// <summary>
        /// caption elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhCaption CreateCaption()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhCaption(writer, this);
        }
    }
    /// <summary>
    /// div elemnt output class
    /// </summary>
    public class NhDiv : NhBlock
    {
        /// <summary>
        /// NhDiv class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhDiv(TextWriter writer, NhBase parent)
            : base(writer, "div", parent)
        {
        }
    }
    /// <summary>
    /// blockquote elemnt output class
    /// </summary>
    public class NhBlockQuote : NhBlock
    {
        /// <summary>
        /// NhBlockQuote class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhBlockQuote(TextWriter writer, NhBase parent)
            : base(writer, "blockquote", parent)
        {
        }
    }
    /// <summary>
    /// h1Å`h6 elemnt output class
    /// </summary>
    public class NhHx : NhInline
    {
        /// <summary>
        /// NhHx class constructor
        /// </summary>
        /// <param name="level">1 to 6 number in 2nd character if tag name</param>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhHx(int level, TextWriter writer, NhBase parent)
            : base(writer, "h" + level.ToString(), parent)
        {
            System.Diagnostics.Debug.Assert(level >= 1 && level <= 6);
        }
    }
    /// <summary>
    /// div elemnt output class
    /// </summary>
    public class NhForm : NhBlock
    {
        /// <summary>
        /// NhForm class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        /// <param name="action">action attribute value</param>
        public NhForm(TextWriter writer, NhBase parent, string action)
            : base(writer, "form", parent)
        {
            this.WriteAttribute("action", action);
        }
        /// <summary>
        /// outout method attribute with post value
        /// </summary>
        public void WritePostMethod()
        {
            this.WriteAttribute("method", "post");
        }
        /// <summary>
        /// outout method attribute with get value
        /// </summary>
        public void WriteGetMethod()
        {
            this.WriteAttribute("method", "get");
        }
    }
    /// <summary>
    /// input elemnt output class
    /// </summary>
    public class NhInput : NhEmpty
    {
        /// <summary>
        /// NhInput class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhInput(TextWriter writer, NhBase parent)
            : base(writer, "input", parent)
        {
        }
    }
    /// <summary>
    /// label elemnt output class
    /// </summary>
    public class NhLabel : NhInline
    {
        /// <summary>
        /// NhLabel class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhLabel(TextWriter writer, NhBase parent)
            : base(writer, "label", parent)
        {
        }
    }
    /// <summary>
    /// select elemnt output class
    /// </summary>
    public class NhSelect : NhInline
    {
        /// <summary>
        /// option elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhOption CreateOption()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhOption(writer, this);
        }
        /// <summary>
        /// NhSelect class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhSelect(TextWriter writer, NhBase parent)
            : base(writer, "select", parent)
        {
        }
    }
    /// <summary>
    /// option elemnt output class
    /// </summary>
    public class NhOption : NhTextAvailable
    {
        /// <summary>
        /// NhSelect class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhOption(TextWriter writer, NhBase parent)
            : base(writer, "option", parent)
        {
        }
    }
    /// <summary>
    /// textarea elemnt output class
    /// </summary>
    public class NhTextArea : NhTextAvailable
    {
        /// <summary>
        /// NhTextArea class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        /// <param name="cols">cols attribute value</param>
        /// <param name="rows">rows attribute value</param>
        public NhTextArea(TextWriter writer, NhBase parent, int cols, int rows)
            : base(writer, "textarea", parent)
        {
            this.WriteAttribute("cols", cols.ToString());
            this.WriteAttribute("rows", rows.ToString());
        }
    }

    /// <summary>
    /// generic class for block content element. Use by derive
    /// </summary>
    public class NhBlock : NhNotEmpty
    {
        /// <summary>
        /// NhBlock class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="tagName">tag name, if you use namespace prifx, use PREFIX:NAME style.</param>
        /// <param name="parent">parent element object</param>
        public NhBlock(TextWriter writer, string tagName, NhBase parent)
            : base(writer, tagName, parent)
        {
        }
        /// <summary>
        /// p elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhP CreateP()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhP(writer, this);
        }
        /// <summary>
        /// ul elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhUl CreateUl()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhUl(writer, this);
        }
        /// <summary>
        /// ol elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhOl CreateOl()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhOl(writer, this);
        }
        /// <summary>
        /// table elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhTable CreateTable()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhTable(writer, this);
        }
        /// <summary>
        /// div elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhDiv CreateDiv()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhDiv(writer, this);
        }
        /// <summary>
        /// blockquote elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhBlockQuote CreateBlockQuote()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhBlockQuote(writer, this);
        }
        /// <summary>
        /// h1Å`h6 elemnt object creation
        /// </summary>
        /// <param name="level">1 to 6 integer value of 2nd character of tag name.</param>
        /// <returns>created object</returns>
        public NhHx CreateHx(int level)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhHx(level, writer, this);
        }
        /// <summary>
        /// Form elemnt object creation
        /// </summary>
        /// <param name="action">action attribute value</param>
        /// <returns>created object</returns>
        public NhForm CreateForm(string action)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhForm(writer, this, action);
        }
        /// <summary>
        /// output p element which has text content without any attribute
        /// </summary>
        /// <param name="text">text for content</param>
        public void WritePText(string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<p>");
            writer.Write(NhUtil.QuoteText(text));
            writer.Write("</p>\r\n");
        }
        /// <summary>
        /// output h1 to h6 element which has text content without any attribute
        /// </summary>
        /// <param name="level">1 to 6 integer value of 2nd character of tag name.</param>
        /// <param name="text">text for content</param>
        public void WriteHxText(int level, string text)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<h");
            writer.Write(level);
            writer.Write(">");
            writer.Write(NhUtil.QuoteText(text));
            writer.Write("</h");
            writer.Write(level);
            writer.Write(">\r\n");
        }
    }
    /// <summary>
    /// body elemnt output class
    /// </summary>
    public class NhBody : NhBlock
    {
        /// <summary>
        /// NhBody class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhBody(TextWriter writer, NhBase parent)
            : base(writer, "body", parent)
        {
        }
    }
    /// <summary>
    /// head elemnt output class
    /// </summary>
    public class NhHead : NhNotEmpty
    {
        /// <summary>
        /// NhHead class constructor
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        public NhHead(TextWriter writer, NhBase parent)
            : base(writer, "head", parent)
        {
        }
        /// <summary>
        /// output title element which has text content without any attribte
        /// </summary>
        /// <param name="title">text for content</param>
        public void WriteTitle(string title)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<title>");
            writer.Write(NhUtil.QuoteText(title));
            writer.Write("</title>\r\n");
        }
        /// <summary>
        /// output meta element with Content-Type information
        /// </summary>
        /// <remarks>output follow format
        /// &lt;meta http-equiv="Content-Type" content="text/html;charset="YOUR VALUE" />
        /// </remarks>
        /// <param name="encodingName">charset name</param>
        public void WriteEncodingName(string encodingName)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=");
            writer.Write(encodingName);
            writer.Write("\" />\r\n");
        }
        /// <summary>
        /// output reference information for style-sheet
        /// </summary>
        /// <remarks>output follow format
        /// &lt;link href="HREF VALUE" type="TYPE VALUE" rel="stylesheet" />
        /// </remarks>
        /// <param name="href">URI of style-sheet. if null, no style-sheet information generate</param>
        /// <param name="type">media-type of style-sheet (example:text/css)</param>
        /// <param name="requireSlash">require / before &gt;</param>
        public void WriteStyleSheetRefer(string href, string type, bool requireSlash)
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            writer.Write("<link href=\"");
            writer.Write(NhUtil.QuoteText(href));
            writer.Write("\" type=\"");
            writer.Write(NhUtil.QuoteText(type));
            writer.Write("\" rel=\"stylesheet\"");
            if (requireSlash) writer.Write(" /");
            writer.Write(">\r\n");
        }
        /// <summary>
        /// output reference information for style-sheet
        /// </summary>
        /// <param name="href">URI of style-sheet. if null, no style-sheet information generate</param>
        /// <param name="type">media-type of style-sheet (example:text/css)</param>
        [Obsolete]
        public void WriteStyleSheetRefer(string href, string type)
        {
            WriteStyleSheetRefer(href, type, true);
        }
    }
    /// <summary>
    /// html elemnt output class
    /// </summary>
    public class NhHtml : NhNotEmpty
    {
        /// <summary>
        /// NhHtml class constructor
        /// </summary>
        /// <remarks>if xml style, xmlns attribute is specified. the value is http://www.w3.org/1999/xhtml</remarks>
        /// <param name="writer">writer for output</param>
        /// <param name="parent">parent element object</param>
        /// <param name="requireNamespace">require xmlns attribute for http://www.w3.org/1999/xhtml</param>
        public NhHtml(TextWriter writer, NhBase parent, bool requireNamespace)
            : base(writer, "html", parent)
        {
            if (requireNamespace) WriteAttribute("xmlns", "http://www.w3.org/1999/xhtml");
        }
        /// <summary>
        /// head elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhHead CreateHead()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhHead(writer, this);
        }
        /// <summary>
        /// body elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhBody CreateBody()
        {
            if (NhSetting.StrictNestChecking) checkLock();
            confirmStartTagClose();
            return new NhBody(writer, this);
        }
    }
    /// <summary>
    /// entire document
    /// </summary>
    public class NhDocument : NhBase
    {
        /// <summary>
        /// NhDocument class constructor
        /// automatically create XML declalation and DOCTYPE declalation
        /// </summary>
        /// <remarks>in case of type of documents, follow text will output.
        /// In case of NhDocumentType.Xhtml11
        /// &lt;?xml version=\"1.0\"?>
        /// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
        /// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
        /// 
        /// In case of NhDocumentType.Xhtml10Strict
        /// &lt;?xml version=\"1.0\"?>
        /// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
        /// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"> 
        /// 
        /// In case of NhDocumentType.Xhtml10Transitional
        /// &lt;?xml version=\"1.0\"?>
        /// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
        /// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
        /// 
        /// In case of NhDocumentType.Html5
        /// &lt;!DOCTYPE html>
        /// </remarks>
        /// <param name="writer">writer for output</param>
        /// <param name="documentType">type of document</param>
        public NhDocument(TextWriter writer, NhDocumentType documentType)
            : base(writer, null)
        {
            if (documentType != NhDocumentType.Html5) writer.WriteLine("<?xml version=\"1.0\"?>");
            switch (documentType)
            {
                case NhDocumentType.Xhtml11:
                    writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
                    writer.WriteLine("\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
                    break;
                case NhDocumentType.Xhtml10Strict:
                    writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"");
                    writer.WriteLine("\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
                    break;
                case NhDocumentType.Xhtml10Transitional:
                    writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
                    writer.WriteLine("\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                    break;
                case NhDocumentType.Html5:
                    writer.WriteLine("<!DOCTYPE html>");
                    break;
            }
        }
        /// <summary>
        /// NhDocument class constructor
        /// automatically create XML declalation and DOCTYPE declalation for XHTML 1.1
        /// </summary>
        /// <remarks>output follow lines
        /// &lt;?xml version=\"1.0\"?>
        /// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
        /// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
        /// </remarks>
        /// <param name="writer">writer for output</param>
        public NhDocument(TextWriter writer)
            : base(writer, null)
        {
            writer.WriteLine("<?xml version=\"1.0\"?>");
            writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
            writer.WriteLine("\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
        }
        /// <summary>
        /// html elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        public NhHtml CreateHtml(bool requireNamespace)
        {
            return new NhHtml(writer, this, requireNamespace);
        }
        /// <summary>
        /// html elemnt object creation
        /// </summary>
        /// <returns>created object</returns>
        [Obsolete]
        public NhHtml CreateHtml()
        {
            return new NhHtml(writer, this, true);
        }
    }
    /// <summary>
    /// Quick Generate Document, which output common header
    /// </summary>
    public class NhQuickDocument : IDisposable
    {
        private NhHtml html;
        private NhBody body;
        /// <summary>
        /// NhQuickDocument class constructor
        /// Header informations for parameters
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="title">title of document (title element)</param>
        /// <param name="styleSheetHref">URI of style-sheet. if null, there is no style-sheet</param>
        /// <param name="styleSheetType">Media Type of style-sheet (esample:text/css)</param>
        [System.Obsolete()]
        public NhQuickDocument(TextWriter writer, string title,
            string styleSheetHref, string styleSheetType)
        {
            NhDocument nestedDocument = new NhDocument(writer);
            html = nestedDocument.CreateHtml(true);
            using (NhHead head = html.CreateHead())
            {
                head.WriteTitle(title);
                if (styleSheetHref != null)
                {
                    head.WriteStyleSheetRefer(styleSheetHref, styleSheetType);
                }
            }
            body = html.CreateBody();
        }
        /// <summary>
        /// NhQuickDocument class constructor
        /// Header informations for parameters
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="title">title of document (title element)</param>
        /// <param name="styleSheetHref">URI of style-sheet. if null, there is no style-sheet</param>
        /// <param name="styleSheetType">Media Type of style-sheet (esample:text/css)</param>
        /// <param name="languageCode">Language code for document default. (example:us-EN)</param>
        /// <param name="documentType">type of document</param>
        /// <remarks>if type is NhDocumentType.Xhtml10Strict or NhDocumentType.Xhtml10Transitional,
        /// language code will inlcude in lang attribute, as well as xml:lang attribute 
        /// </remarks>
        public NhQuickDocument(TextWriter writer, string title,
            string styleSheetHref, string styleSheetType, string languageCode, NhDocumentType documentType)
        {
            NhDocument nestedDocument = new NhDocument(writer, documentType);
            html = nestedDocument.CreateHtml(documentType != NhDocumentType.Html5);
            writeLang(languageCode, documentType);
            using (NhHead head = html.CreateHead())
            {
                head.WriteTitle(title);
                if (styleSheetHref != null)
                {
                    head.WriteStyleSheetRefer(styleSheetHref, styleSheetType, documentType != NhDocumentType.Html5);
                }
            }
            body = html.CreateBody();
        }

        private void writeLang(string languageCode, NhDocumentType documentType)
        {
            if (documentType != NhDocumentType.Xhtml11) html.WriteAttribute("lang", languageCode);
            if (documentType != NhDocumentType.Html5) html.WriteAttribute("xml:lang", languageCode);
        }
        /// <summary>
        /// NhQuickDocument class constructor
        /// Header informations for parameters
        /// </summary>
        /// <param name="writer">writer for output</param>
        /// <param name="title">title of document (title element)</param>
        /// <param name="styleSheetHref">URI of style-sheet. if null, there is no style-sheet</param>
        /// <param name="styleSheetType">Media Type of style-sheet (esample:text/css)</param>
        /// <param name="languageCode">Language code for document default. (example:us-EN)</param>
        /// <param name="documentType">type of document</param>
        /// <param name="encodingName">IANA charset name for header</param>
        /// <remarks>if type is NhDocumentType.Xhtml10Strict or NhDocumentType.Xhtml10Transitional,
        /// language code will inlcude in lang attribute, as well as xml:lang attribute 
        /// </remarks>
        public NhQuickDocument(TextWriter writer, string title,
            string styleSheetHref, string styleSheetType, string languageCode, NhDocumentType documentType,
            string encodingName)
        {
            NhDocument nestedDocument = new NhDocument(writer, documentType);
            html = nestedDocument.CreateHtml(documentType != NhDocumentType.Html5);
            writeLang(languageCode, documentType);
            using (NhHead head = html.CreateHead())
            {
                head.WriteRawString("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"");
                if (documentType == NhDocumentType.Xhtml10Strict || documentType == NhDocumentType.Xhtml10Transitional)
                {
                    head.WriteRawString(" /");
                }
                head.WriteRawString(">\r\n");
                head.WriteTitle(title);
                if (styleSheetHref != null)
                {
                    head.WriteStyleSheetRefer(styleSheetHref, styleSheetType, documentType != NhDocumentType.Html5);
                }
            }
            body = html.CreateBody();
        }
        /// <summary>
        /// Reference for instance of NhBody class in this document.
        /// Use this attribute to outout document body.
        /// </summary>
        public NhBody B
        {
            get { return body; }
        }
        /// <summary>
        /// Release allocated resources
        /// </summary>
        public void Dispose()
        {
            body.Dispose();
            html.Dispose();
        }
    }
}
