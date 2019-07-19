using System.Globalization;

namespace ShapeFlow.RuleEngines.T4
{
    internal class ArgumentInfo
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                FieldName = string.Format(CultureInfo.InvariantCulture, "_{0}", _name);
            }
        }
        
        public string Type { get; set; }
        
        public string FieldName { get; private set; }
        
        public string ConverterType { get; set; }
    }
}