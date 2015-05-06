//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Tie
{
    class ContextInstance
    {
        public readonly Context context;
        public readonly VAL instance;

        public ContextInstance(Context context, VAL instance)
        {
            this.context = context;
            this.instance = instance;
        }
    }

    class HostEvent
    {
        public const string EVENT_HANDLER_NAME = "$EventHandlers";
        public static bool RemoveEventHandlerSupported = false;
        
        EventInfo eventInfo;
        VAL func;
        VAL ret;
        Context context;
        VAL instance;

        Memory DS2;
       

        public HostEvent(EventInfo eventInfo, VAL func)
        {
            this.eventInfo = eventInfo;
            this.func = func;
            ContextInstance temp = (ContextInstance)func.temp;
            this.context = temp.context;
            this.instance = temp.instance;

            this.DS2 = this.context.DataSegment;
        }

        public VAL AddDelegateEventHandler()
        {
            MethodInfo methodInfo = this.GetType().GetMethod("Callback", BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate dEmitted = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            SaveEventHandler(dEmitted);

            VAL dVAL = VAL.NewHostType(dEmitted);
            dVAL.temp = this;
            dVAL.hty = HandlerActionType.Add;   //using SEGREG.DS as flag of ADD HANDLER 
            return dVAL;
        }


        public VAL RemoveDelegateEventHandler()
        {
            Delegate dEmitted = LoadEventHandler();

            VAL dVAL = VAL.NewHostType(dEmitted);
            dVAL.temp = this;
            dVAL.hty = HandlerActionType.Remove;   //using SEGREG.NS as flag of REMOVE HANDLER 
            return dVAL;
        }



        private void Callback(object sender, EventArgs e)
        {
            VALL L = new VALL();
            L.Add(VAL.NewHostType(sender));
            L.Add(VAL.NewHostType(e));
            VAL arguments = new VAL(L);
            ret = CPU.ExternalUserFuncCall(func, instance, arguments, context);
        }


        #region DynamicMethod
        //public VAL AddEventHandler()
        //{

        //    Type handlerType = eventInfo.EventHandlerType;

        //    //Type returnType = GetDelegateReturnType(handlerType);
        //    //if (returnType != typeof(void))
        //    //    throw new ApplicationException("Delegate has a return type.");

        //    DynamicMethod handler = new DynamicMethod(
        //                            "EventHandler",
        //                            null,
        //                            GetDelegateParameterTypes(handlerType),
        //                            typeof(HostEvent));

        //    // Generate a method body. 
        //    // pops the return value off the stack (because the handler has no return type), and returns.
        //    ILGenerator ilgen = handler.GetILGenerator();

        //    Type[] parameters = { typeof(object), typeof(object) };
        //    MethodInfo method = typeof(HostEvent).GetMethod("Callback");//, BindingFlags.Instance | BindingFlags.NonPublic);

        //    ilgen.Emit(OpCodes.Nop);
        //    ilgen.Emit(OpCodes.Ldarg_0);
        //    ilgen.Emit(OpCodes.Ldarg_1);
        //    ilgen.Emit(OpCodes.Ldarg_2);
        //    ilgen.Emit(OpCodes.Call, method);

        //    ilgen.Emit(OpCodes.Stloc_0);
        //    Label IL_000c = ilgen.DefineLabel();
        //    ilgen.Emit(OpCodes.Br_S, IL_000c);
        //    ilgen.MarkLabel(IL_000c);
        //    ilgen.Emit(OpCodes.Ldloc_0);
        //    ilgen.Emit(OpCodes.Ret);



        //    Delegate dEmitted = handler.CreateDelegate(handlerType);
        //    VAL dVAL = VAL.NewHostType(dEmitted);
        //    dVAL.Temp = this;
        //    return dVAL;
        //}


        //private static Type[] GetDelegateParameterTypes(Type d)
        //{
        //    if (d.BaseType != typeof(MulticastDelegate))
        //        throw new ApplicationException("Not a delegate.");

        //    MethodInfo invoke = d.GetMethod("Invoke");
        //    if (invoke == null)
        //        throw new ApplicationException("Not a delegate.");

        //    ParameterInfo[] parameters = invoke.GetParameters();
        //    Type[] typeParameters = new Type[parameters.Length];
        //    for (int i = 0; i < parameters.Length; i++)
        //    {
        //        typeParameters[i] = parameters[i].ParameterType;
        //    }
        //    return typeParameters;
        //}

        //private static Type GetDelegateReturnType(Type d)
        //{
        //    if (d.BaseType != typeof(MulticastDelegate))
        //        throw new ApplicationException("Not a delegate.");

        //    MethodInfo invoke = d.GetMethod("Invoke");
        //    if (invoke == null)
        //        throw new ApplicationException("Not a delegate.");

        //    return invoke.ReturnType;
        //}

        #endregion




        //把Event Handler保存至CPU.DS2中,那么清除CPU.DS2,就可以清除Event Handler
        private Dictionary<EventInfo, Delegate> delegates
        {
            get
            {
                if (DS2 == null)
                    return null;

                if (!DS2.Exists(EVENT_HANDLER_NAME))
                    DS2.AddHostObject(EVENT_HANDLER_NAME, new Dictionary<EventInfo, Delegate>());

                return (Dictionary<EventInfo, Delegate>)DS2[EVENT_HANDLER_NAME].value;
            }
        }

        private void SaveEventHandler(Delegate dEmitted)
        {
            if (!RemoveEventHandlerSupported)
                return;

            if (delegates != null)
            {
                if (delegates.ContainsKey(eventInfo))
                    delegates.Remove(eventInfo);

                delegates.Add(eventInfo, dEmitted);
            }
        }

        private Delegate LoadEventHandler()
        {
            if (!RemoveEventHandlerSupported)
                new HostTypeException("Remove EventHandler is not supported.");

            if (delegates != null && delegates.ContainsKey(eventInfo))
            {
                Delegate dEmitted = delegates[eventInfo]; ;

                if (delegates.ContainsKey(eventInfo))
                    delegates.Remove(eventInfo);
                
                return dEmitted;
            }

            throw new HostTypeException("Can't remove not existed EventHandler");
        }

        public void ClearEventHandler()
        {
            ClearEventHandler(DS2);
        }

        public static void ClearEventHandler(Memory DS2)
        {
            if (DS2.Exists(HostEvent.EVENT_HANDLER_NAME))
                DS2.Remove(HostEvent.EVENT_HANDLER_NAME);
        }

    }
}
