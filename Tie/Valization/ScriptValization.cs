﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Valization
{

    class ScriptValization : BaseValization
    {
        private string valizer;
        private string devalizer;

        public ScriptValization(string valizer, string devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        protected override VAL valize(object host)
        {
            VAL val = Script.Run(host, valizer, new Memory());
            return val;

        }


        protected override object devalize(object host, VAL val)
        {
            if (devalizer == null)
                return host;

            Memory DS = new Memory();
            DS.Add(Constant.THIS, val);
            VAL x = Script.Run(host, devalizer, DS);
            return x.HostValue;
        }

    }

}
