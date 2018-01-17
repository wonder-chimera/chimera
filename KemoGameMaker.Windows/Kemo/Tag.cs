using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kemo
{
    public class Tag :IEqualityComparer
    {
        public Tag(string tagName)
        {
            if(tagName.Length == 0)
            {
                throw new ArgumentException(nameof(tagName)+"の長さは１以上である必要があります。");
            }
            TagName = tagName ?? throw new ArgumentNullException(nameof(tagName));
        }

        public string TagName { get; }

        public new bool Equals(object x, object y)
        {
            if(x == null || y == null)
            {
                return x == y;
            }

            return ((Tag)x).TagName == ((Tag)y).TagName;
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return -1;
            }

            return 0;
        }
    }
}
