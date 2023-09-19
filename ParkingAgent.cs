// ParkingAgent (N := 5):
//      Event-Driven class!
//      If price-Cut occurs, call CallBack().
//      Makes Order object
//      [EVENT-ORIENTED] Read new price based on CallBack price rewrite. 
//      Sends order to ParkingStructure thru MultiCellBuffer
//      CallBack method (Event-Handler):
//          writes new reduced price into var that is read by ParkingAgent
//      Terminates after ParkingStructure threads have terminated
//      once calculations have been made:
//          Send to MultiCellBuffer
namespace Parking_Service {

    internal class ParkingAgent {
        private int IDIncrementor = 0;
        public bool TerminateThreads = false;

        private MultiCellBuffer orderBuffer;
        private ParkingStructure parkingStructure;

        private float receivedPrice = -1;
        private float budget;               // randomly generated number to randomize purchase limits. 
        private int desiredParkingSpaces;   // how many parking spaces this agent wants

        private Order currentOrder;

        private Random random;

        // Constructor initializes thread object, shared MultiCellBuffer
        public ParkingAgent(MultiCellBuffer orderBuffer) {
            this.orderBuffer = orderBuffer;
        }

        //creates an order from user requests. DOES NOT APPLY PARKINGSTRUCTURE EXTRA FEES
        public void GenerateOrder(int cardNumber, int quantity, float price) {
            // get inputs from user in main to set up a new order
            currentOrder = new Order();            
            currentOrder.SetCardNumber(cardNumber);     // User inputs
            currentOrder.SetQuantity(quantity);
            currentOrder.SetID(this.IDIncrementor++);   // order ID's are linear
            currentOrder.SetUnitPrice(price);
        }

        // Creates a random budget to see how many parking spaces an agent can get
        private void CalculateBudget() {
            random = new Random();
            double randomDouble = random.NextDouble();

            //calculates random float between 30 - 100
            randomDouble = 30.0f + random.NextDouble() * 70.0f;
            budget = (float)Math.Round(randomDouble, 2);
        }

        // Decides how many parking spaces an agent may want (between 1-3 right now)
        private void CalculateDesiredParkingSpaces() {
            random = new Random();
            desiredParkingSpaces = random.Next(1, 3);
        }

        // used to activate Run() if PriceDropHandler is triggered to reduce wasted time
        private AutoResetEvent PriceDropEvent = new AutoResetEvent(false);

        // Receives invocation from ParkingStructure and starts thread to make order based on this price. 
        public void PriceDropHandler(float parkingSpacePrice, bool TerminateThreads) {
            receivedPrice = parkingSpacePrice;
            this.TerminateThreads = TerminateThreads;

            if(parkingSpacePrice != 1000)
                Console.WriteLine($"\t\tPrice Dropped on {Thread.CurrentThread.Name}!\n" +
                                  $"\t\tNew Price: {receivedPrice}\n");

            PriceDropEvent.Set();
        }

        // Method to be started by Thread in main
        public void Run() {
            Console.WriteLine($"\t\tParking Agent thread {Thread.CurrentThread.Name} has started! ----------------------------\n");


            // Calculate the max amount of money an agent can spend and the amount of parking spaces they/them wants. 
            CalculateBudget();
            CalculateDesiredParkingSpaces();

            while(!TerminateThreads) {  //temp
                PriceDropEvent.WaitOne(5000);
                if (TerminateThreads) {
                    Console.WriteLine($"{Thread.CurrentThread.Name} has been signaled to terminate! Exiting. . .");
                    return;
                }
                else if (receivedPrice != -1) {
                    if (budget >= receivedPrice * desiredParkingSpaces) {

                        // Make the order (into currentOrder) and send into buffer
                        GenerateOrder(random.Next(5000, 7000), desiredParkingSpaces, receivedPrice);
                        orderBuffer.SetOneCell(currentOrder);
                    }
                    else {
                        Console.WriteLine($"\t\tThread {Thread.CurrentThread.Name} rejected the price drop\n");
                    }
                }
                else Console.WriteLine($"\t\tThread {Thread.CurrentThread.Name} Currently Waiting. . .");
            }
            // Waits for the parking structure thread to terminate after doing it's job
        }
    }
}
