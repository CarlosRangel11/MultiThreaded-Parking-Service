using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Order: Objects created by ParkingAgents to be placed within the MultiCellBuffer
//      DATAMEMBERS:
//          string/int senderID   = identity of sender (use thread name or id)
//          int        cardNo     = card number
//          string     receiverID = [OPTIONAL] identity of thread from ParkingStructure
//          int        quantity   = # of parking spaces to order
//          double     unitPrice  = price of parking spaces from ParkingStructure
//      Getters/Setters for these value
//      FIND OUT IF METHODS NEED TO BE SYNCED
namespace Parking_Service {
    internal class Order {
        private int ID;              // sender identity
        private int cardNumber;      // credit card number (represented by num between 4000-8000)
        private int quantity;        // number of parking spaces to order
        private float unitPrice;     // price of parking spaces from ParkingStructure

        // Constructor sets the data to sentinal values (-1)
        public Order() {
            this.ID = -1;
            this.cardNumber = -1;
            this.quantity = -1;
            this.unitPrice = -1;
        }

        // getters
        public int GetID() { return this.ID; }
        public int GetCardNumber() { return this.cardNumber; }        
        public int GetQuantity() { return this.quantity; }        
        public float GetUnitPrice() { return this.unitPrice; }

        // setters
        public void SetID(int ID) { this.ID = ID; }
        public void SetCardNumber(int CardNumber) { this.cardNumber = CardNumber; }
        public void SetQuantity(int Quantity) { this.quantity = Quantity; }
        public void SetUnitPrice(float unitPrice) { this.unitPrice = unitPrice;  }
    }
}
