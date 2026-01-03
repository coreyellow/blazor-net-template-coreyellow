// WebSocket connection for real-time TODO updates
window.TodoWebSocket = {
    socket: null,
    messageCallback: null,

    connect: function (dotNetHelper) {
        if (this.socket && this.socket.readyState === WebSocket.OPEN) {
            console.log('WebSocket already connected');
            return;
        }

        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        const wsUrl = `${protocol}//${window.location.host}/ws/todos`;
        
        console.log('Connecting to WebSocket:', wsUrl);
        this.socket = new WebSocket(wsUrl);
        this.messageCallback = dotNetHelper;

        this.socket.onopen = (event) => {
            console.log('WebSocket connected');
        };

        this.socket.onmessage = (event) => {
            console.log('WebSocket message received:', event.data);
            try {
                const message = JSON.parse(event.data);
                if (this.messageCallback) {
                    this.messageCallback.invokeMethodAsync('OnWebSocketMessage', message);
                }
            } catch (error) {
                console.error('Error parsing WebSocket message:', error);
            }
        };

        this.socket.onerror = (error) => {
            console.error('WebSocket error:', error);
        };

        this.socket.onclose = (event) => {
            console.log('WebSocket disconnected', event);
            // Attempt to reconnect after 3 seconds
            setTimeout(() => {
                if (this.messageCallback) {
                    console.log('Attempting to reconnect...');
                    this.connect(this.messageCallback);
                }
            }, 3000);
        };
    },

    disconnect: function () {
        if (this.socket) {
            console.log('Disconnecting WebSocket');
            this.socket.close();
            this.socket = null;
            this.messageCallback = null;
        }
    }
};
