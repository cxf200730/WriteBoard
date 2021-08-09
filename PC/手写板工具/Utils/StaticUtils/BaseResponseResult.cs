using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.StaticUtils
{
    public class BaseResponseResult
    {
        public BaseResponseState State;
        public string Message;
        public object Object;

        public static BaseResponseResult Success(object objects)
        {
            return new BaseResponseResult { State = BaseResponseState.Success, Message = "Success", Object = objects };
        }

        public static BaseResponseResult Information(String message)
        {
            return new BaseResponseResult { State = BaseResponseState.Information, Message = message };
        }

        public static BaseResponseResult Wrong(String message)
        {
            return new BaseResponseResult { State = BaseResponseState.Wrong, Message = message };
        }

        public static BaseResponseResult Failed(String message)
        {
            return new BaseResponseResult { State = BaseResponseState.Failed, Message = message };
        }
    }
}