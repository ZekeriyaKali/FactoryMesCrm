namespace FactoryMesCrm.Domain.ValueObjects
{
    public record Quantity
    {
        public int Value { get; }
        
        public Quantity(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Miktar negatif olamaz", nameof(value));
            }
            Value = value;
        }

        public Quantity Add(int amount) => new(Value + amount);

    }
}
