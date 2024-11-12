namespace Solitaire
{
    public class Card
    {
        private Suit _suit = Suit.Heart;
        private Number _number = Number.A;

        public Suit Suit
        {
            get { return _suit; }
            set { _suit = value; }
        }

        public Number Number
        {
            get { return _number; }
            set { _number = value; }
        }
    }
}
