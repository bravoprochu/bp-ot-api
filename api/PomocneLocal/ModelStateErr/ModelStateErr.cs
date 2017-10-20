using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace bp.PomocneLocal.ModelStateHelpful
{
    public static class ModelStateHelpful
    {
        public static ModelStateDictionary ModelError(string extraInfo=null) {
            extraInfo = String.IsNullOrWhiteSpace(extraInfo) ? null : ", " + extraInfo;
            var modelState = new ModelStateDictionary();
            modelState.TryAddModelError("Dane", $"Przesłane dane są nieprawidłowe{extraInfo}");
            return modelState;
        }

        public static ModelStateDictionary ModelError(string errorType, string errorsInfo)
        {
            var modelState = new ModelStateDictionary();
            modelState.TryAddModelError(errorType, errorsInfo);
            return modelState;
        }

        public static void ModelError(ModelStateDictionary modelState, string errorType, string info) {
                modelState.TryAddModelError(errorType, info);
        }

        public static ModelStateDictionary ModelError( Dictionary<string, string> errorDictionary) {
            var modelState = new ModelStateDictionary();
            foreach (var err in errorDictionary)
            {
                modelState.TryAddModelError(err.Key, err.Value);
            }   
            return modelState;
        }

    }
}
