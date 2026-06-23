using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSimulation.Lib
{
    public abstract class User
    {
        public static User One = null;
        
        public enum YesNoQuestion { Yes, No };
        public enum YesNoCancelQuestion { Yes, No, Cancel }
        public enum UpdateErrorQuestion { Skip, SkipRemain, Stop }
        /// <summary>
        /// Tell the user to begin in waiting state
        /// </summary>
        /// <param name="reason">the reason to wait, as a text message sent to the user</param>
        /// <param name="maximumStep">starting indicator</param>
        /// <param name="minimumStep">ending indicator </param>
        public abstract void BeginWait(string reason, int maximumStep, int minimumStep=0);

        public abstract void EndWait(string reason);

        public abstract void ShowWaitIndicator(int atStep, int totalStep);
        
        public abstract void MessageToUser(string strMessage);

        public abstract void SendErrorToUser(Exception ex);
        
        public abstract void SendCalculationResult(string Result);


        public abstract YesNoQuestion AskUserYesNoQuestion(string strMessage);

        public abstract YesNoCancelQuestion AskUserYesNoCancelQuestion(string strMessage);

        public abstract YesNoQuestion AskUserAnValue(string Comment, string Prompt, System.Type type, object DefaultValue, ref object Return);

        public abstract void Clear();
        public abstract void PressAnyKey();
        public abstract bool LockWritingMessage();//Lock or unlock writing message
		
    }
}
