using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public struct Variable
    {
        private Object value;

        public Object Value
        {
            get { return value; }
            set
            {
                if (value is int) 
                    { this.value = double.Parse(value.ToString()); }
                else
                    { this.value = value; }
            }
        }

        public Variable Add(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = (double)this.Value + (double)other.Value;
                return retVal;
            }
            else
            {
                throw new BeeVMException("Add Value can't be done on types different than Number");
            }
        }

        public Variable Subtract(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = (double)this.Value - (double)other.Value;
                return retVal;
            }
            else
            {
                throw new BeeVMException("Subtract Value can't be done on types different than Number");
            }
        }

        public Variable Multiply(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = (double)this.Value * (double)other.Value;
                return retVal;
            }
            else
            {
                throw new BeeVMException("Multiply Value can't be done on types different than Number");
            }
        }

        public Variable Divide(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = (double)this.Value / (double)other.Value;
                return retVal;
            }
            else
            {
                throw new BeeVMException("Divide Value can't be done on types different than Number");
            }
        }

        public Variable Negated()
        {
            if (Value is Boolean)
            {
                Variable retValue = new Variable();
                retValue.Value = !((bool)this.Value);
                return retValue;
            }
            else if( Value is int)
            {
                Variable retValue = new Variable();
                retValue.Value = -(int)this.Value;
                return retValue;
            }
            else
            {
                throw new BeeVMException("Can't negate a value different than Boolean or Number");
            }
        }

        public Variable Equality(Variable variable)
        {
            Variable retValue = new Variable();

            if (Value.GetType() != variable.Value.GetType())
            {
                retValue.Value = false;
            }
            else
            {
                retValue.Value = Value.Equals(variable.Value);
            }
            return retValue;
        }

        public Variable Greater(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = ((double)this.Value) > ((double)other.Value);
                return retVal;
            }
            else
            {
                throw new BeeVMException("Can't compare values different than Number");
            }
        }

        public Variable GreaterEqual(Variable other)
        {
            if (Value is double && other.Value is double)
            {
                Variable retVal = new Variable();
                retVal.Value = ((double)this.Value) >= ((double)other.Value);
                return retVal;
            }
            else
            {
                throw new BeeVMException("Can't compare values different than Number");
            }
        }

        internal Variable Or(Variable variable)
        {
            Variable retValue = new Variable();

            if (!(Value is Boolean && variable.Value is Boolean))
            {
                throw new BeeVMException("Can't use And operator on variable different than Boolean");
            }
            else
            {
                retValue.Value = (Boolean)Value || (Boolean)variable.Value;
            }
            return retValue;
        }

        internal Variable And(Variable variable)
        {
            Variable retValue = new Variable();

            if (!(Value is Boolean && variable.Value is Boolean))
            {
                throw new BeeVMException("Can't use And operator on variable different than Boolean");
            }
            else
            {
                retValue.Value = (Boolean)Value && (Boolean)variable.Value;
            }
            return retValue;
        }
    }
}
