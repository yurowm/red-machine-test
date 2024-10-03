using Connection;

namespace Events {
    public struct ColorNodeEnable: IEvent {
        public readonly ColorNode node;
        
        public ColorNodeEnable(ColorNode node) {
            this.node = node;
        }
    }
    
    public struct ColorNodeDisable: IEvent {
        public readonly ColorNode node;
        
        public ColorNodeDisable(ColorNode node) {
            this.node = node;
        }
    }
}