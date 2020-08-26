using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuth {

	public class StringValueAttribute : Attribute {
		public StringValueAttribute (string value)
		{
			Value = value;
		}
		public string Value { get; set; }
	}
	public class PathAttribute : StringValueAttribute {
		public PathAttribute (string path) : base (path)
		{
			Value = path;
		}
	}


	public class AcceptsAttribute : StringValueAttribute {
		public AcceptsAttribute (string accepts) : base (accepts)
		{

		}
	}

	public class ContentTypeAttribute : StringValueAttribute {
		public ContentTypeAttribute (string contentType) : base (contentType)
		{

		}
	}
}
