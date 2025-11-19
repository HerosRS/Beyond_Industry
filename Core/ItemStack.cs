namespace BeyondIndustry.Core
{
    /// <summary>
    /// Ein Item mit einer Anzahl (z.B. "5x Ore")
    /// </summary>
    public class ItemStack
    {
        public Item Item { get; }
        public int Count { get; set; }
        
        public ItemStack(Item item, int count = 1)
        {
            Item = item;
            Count = count;
        }
        
        /// <summary>
        /// Fügt Items hinzu
        /// </summary>
        public void Add(int amount)
        {
            Count += amount;
        }
        
        /// <summary>
        /// Entfernt Items (gibt zurück ob es geklappt hat)
        /// </summary>
        public bool Remove(int amount)
        {
            if (Count >= amount)
            {
                Count -= amount;
                return true;
            }
            return false;
        }
        
        public override string ToString()
        {
            return $"{Count}x {Item.Name}";
        }
    }
}