// Carlos Rangel
// ASUID: 1217700029
//
// Parking Space E-Commerce manager that uses multithreading to manage a client-server system (Locally)
// General program used to study different multithreading tools, mehtodologies and programs. 

///////////////////////////////////////////////////////////////////////////////////////////////////////////
// Classes Identified: 
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

// PricingModel:
//      Decides parking spaces price
//      uses math model (or random func) to generate number between 10-40 (MUST FLUCTUATE).

// OrderProcessing: 
//      Makes new thread to process orders. 
//      validates credit cards (use custom formatting)
//      calculate total charge:
//          ex: unitPrice * #OfParkingSpaces + Tax + LocationCharge
//          (Tax and Location must be represented as random values within given ranges)
//      

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

// Order:
//      DATAMEMBERS:
//          string/int senderID   = identity of sender (use thread name or id)
//          int        cardNo     = card number
//          string     receiverID = [OPTIONAL] identity of thread from ParkingStructure
//          int        quantity   = # of parking spaces to order
//          double     unitPrice  = price of parking spaces from ParkingStructure
//      Getters/Setters for these value
//      FIND OUT IF METHODS NEED TO BE SYNCED

// MultiCellBuffer:
//      Comm line between ParkingAgents and ParkingStructures
//      3 Data Cells:
//          each a reference to an Order obj.
//          enforced by semaphore n = 3
//          Locks for read/write permissions between threads
//          CANNOT USE QUEUE
//          can read at the same time
//          no read/write || write/write
//          IF message gen is slow, use sleep func (prob base^2 growth)
//          make sure cells are used as uniformly distributed as possible
//      setters and getters for cells

// Main:
//      Performs prep
//      Create buffer classes
//      instantiate objects
//      create & start threads
//          
// 
// //////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Parking_Service {

    public class Program {
        public static void Main(string[] args) {

            int K = 1, N = 5;
            MultiCellBuffer buffer = new MultiCellBuffer();
            buffer.InitializeLocks();
            ParkingStructure parking_structure = new ParkingStructure(buffer);
            ParkingAgent parking_agent = new ParkingAgent(buffer);

            // PriceDropHandler subscribes to OnPriceDropped event
            parking_structure.OnPriceDropped += parking_agent.PriceDropHandler;

            // initialize i parking tructures
            Thread parking_structure_thread;
            for (int i = 1; i <= K; i++) {
                parking_structure_thread = new Thread(new ThreadStart(parking_structure.Run));
                parking_structure_thread.Name = $"Structure-{i}";
                parking_structure_thread.Start();
            }

            // initialize N parking agents
            Thread[] parking_agents = new Thread[N];
            for(int i = 0; i < N; i++) {
                Thread.Sleep(250);
                parking_agents[i] = new Thread(parking_agent.Run);
                parking_agents[i].Name = $"Agent-{i+1}";
                parking_agents[i].Start();
            }

            // terminating all the threads

            Console.WriteLine("End of main Thread. . .");
        }
    }
}