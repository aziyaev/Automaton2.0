using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Labyrinth.Validation
{
    class InputValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            AutomatonInMaze table_input = (value as BindingGroup).Items[0] as AutomatonInMaze;
            if (AutomatonInMaze.Input_Single != "nswe")
            {
                return new ValidationResult(false, "GG");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
