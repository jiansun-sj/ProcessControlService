using System;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    public abstract class CustomizedTypeInstance : ICloneable
    {
        protected ICustomType _ownerType;

        //public CustomizedTypeInstance(ICustomizedType Type)
        //{
        //    _ownerType = Type;
        //}

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        #region "IParameterizedValue"
        
        public virtual string ParameterName => throw new NotImplementedException();

        public virtual object ParameterValue { get; set; }

        //virtual public string ClassPath => throw new NotImplementedException();

        public virtual string ToJson() => throw new NotImplementedException();
        
        #endregion

    
    }
}
