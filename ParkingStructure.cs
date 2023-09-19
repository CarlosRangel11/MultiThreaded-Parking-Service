using System;
using System.Diagnostics;
using System.Threading;

// ParkingStructure (K := 1):
//      Uses pricing model to calc price for block parking spaces to be used by agents
//      Uses Price-Cut Event to emit event (promotion) to subs. Calls event handlers in subbed agents
//      CONDITION: Counter t:
//          After t price cuts, terminate ParkingStructure thread
//      Awaits order from ParkingAgent thru MultiCellBuffer
//      Creates new Thread => OrderProcessingThread: 
//          process order:
//              Checks card, max purchases allowed
//              calculates total amount
//          sends confirmaton to parkking agent
//          prints order info on screen

namespace Parking_Service {
    // delegate declaration
    public delegate void ParkingSpaceValueDropped(float ParkingSpacePrice, bool TerminateThreads);

    internal class ParkingStructure {

        private MultiCellBuffer orderBuffer;    // use the same buffer
        private float parkingSpacePrice, tax, locationFee;  // various prices
        float previousPrice = 1000;     // Value to compare to each new value of parkingSpacePrice
        bool priceDropped = false;              // condition for emitting price-cut
        int amountOfPriceDrops;         // limit on amount of price drops before thread has to be terminated. 
        Order orderToBeProcessed;

        public bool TerminateThreads = false;
        

        // constructor sets the name (ID) of the thread. 
        public ParkingStructure(MultiCellBuffer orderBuffer) { 
            this.orderBuffer = orderBuffer;
        }
        // have to use getters to keep members private, as I dont
        // want these to be manipulated outside the class
        public float GetParkingSpacePrice() { return parkingSpacePrice; }        

        // event to trigger when the priceDropped variable is true. 
        public event ParkingSpaceValueDropped OnPriceDropped;

        // Alerts the subscribers that a price drop has occured. 
        public void AlertSubscribers(float parkingSpacePrice, bool TerminateThreads) {
            // ? checks if there are any methods subscribed to event
            OnPriceDropped?.Invoke(parkingSpacePrice, TerminateThreads);
        }

        public AutoResetEvent endAllThreads = new AutoResetEvent(false);
         
        public void Run() {     // Method to be started by the thread.
            Console.WriteLine($"Parking structure {Thread.CurrentThread.Name} has started!\n\n");

            for (int t = 0; t < 20; /*if priceDropped: t++*/) {
                Thread.Sleep(1000);
                PricingModel();

                if (priceDropped) { 
                    AlertSubscribers(parkingSpacePrice, TerminateThreads);
                    t++;
                }

                //make a thread to process the order concurrently
                // orderBuffer.PrintBuffer();
                orderToBeProcessed = orderBuffer.GetOneCell();

                // If the retrieved order is not null, process it in a separate thread
                if(orderToBeProcessed != null) {
                    Thread ProcessOrderThread = new Thread(ProcessOrder);
                    ProcessOrderThread.Name = $"{Thread.CurrentThread.Name}::OrderThread";
                    ProcessOrderThread.Start();
                } else
                Console.WriteLine($"{Thread.CurrentThread.Name} Couldn't retrieve order from orderBuffer");

            }
            Console.WriteLine($"Parking Structure {Thread.CurrentThread.Name} has made 20 price cuts. Terminating all threads associated. ");
            TerminateThreads = true;        // next execution of ParkingAgent.Run() will terminate the threads
            AlertSubscribers(1000, TerminateThreads);
            Console.WriteLine($"All Parking Agents have been signaled to end their processes after they complete their tasks!" +
                              $"\n{Process.GetCurrentProcess().Threads.Count} Threads remain active. . .");
            Console.WriteLine($"Parking Structure {Thread.CurrentThread.Name} exiting. . .");
        }

        private void ProcessOrder() {
            Console.WriteLine("Order Received from Parking Agent!");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"Order ID:\t\t\t{orderToBeProcessed.GetID()}");
            Console.WriteLine($"Order Card #\t\t\t{orderToBeProcessed.GetCardNumber()}");
            Console.WriteLine($"Order Quantity\t\t\t{orderToBeProcessed.GetQuantity()}");
            Console.WriteLine($"Unit Price / Parking Space:\t{parkingSpacePrice:F2}\n");

            float finalPrice = ((parkingSpacePrice + locationFee) * orderToBeProcessed.GetQuantity()) * tax;

            Console.WriteLine($"Total Price:\t{finalPrice:F2}\n\n");
        }

        // modifies the following:
        // value of parkingSpacePrice to be somewhere between $ 10 - 40
        // value of tax to be somewhere between 0.08 - 0.12 (%8 - %12)
        // value of locationFee to be somewhere bewteen $ 2 - 8
        private void PricingModel() {
            Random random = new Random();

            // random float between 0.08 - 0.12 for tax
            double randomDouble = random.NextDouble();
            randomDouble *= 0.04;
            randomDouble -= 0.02;
            float randomFloat = (float)Math.Round(randomDouble, 2);
            this.tax = 0.1f + randomFloat;
            this.tax += 1;  // make it a 1.08 - 1.12 to multiply to final price

            // random float between 0 - 3 for locationFee
            randomDouble = random.NextDouble();
            randomDouble *= 6.0;
            randomDouble -= 3.0;
            randomFloat = (float)Math.Round(randomDouble, 2);
            this.locationFee = 5.0f + randomFloat;

            // random value between 10 - 40 for parkingSpacePrice
            randomDouble = random.NextDouble();
            randomDouble *= 30.0;
            randomDouble -= 15.0;
            randomFloat = (float)Math.Round(randomDouble, 2);
            this.parkingSpacePrice = 25.0f + randomFloat;

            //print for debugging
            //Console.WriteLine($"Tax: {this.tax}\nLocation Fee: {this.locationFee}\nParking Space Price: {this.parkingSpacePrice}");

            // If the price dropped, invoke the subscribers. 
            if (previousPrice > this.parkingSpacePrice) priceDropped = true;
            else priceDropped = false;
            previousPrice = this.parkingSpacePrice;
        }
    }
}
