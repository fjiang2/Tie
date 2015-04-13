using System;
using System.Collections.Generic;
using System.Text;

namespace Tie
{
    /// <summary>
    /// 
    /// </summary>
    public struct Member
    {
        private string name;
        private VAL value;

        /// <summary>
        /// 
        /// </summary>
        public string Name 
        { 
            get { return this.name; } 
        }
        
        /// <summary>
        /// 
        /// </summary>
        public VAL Value 
        {
            get { return this.value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Member(string name, VAL value)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}={1}", Name, Value);
        }
    }
}
