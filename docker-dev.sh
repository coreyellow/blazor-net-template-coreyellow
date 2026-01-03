#!/bin/bash
# Development script for running the application with Docker
# Usage: ./docker-dev.sh [command]
# Commands:
#   start    - Build and start the containers (default)
#   stop     - Stop the containers
#   restart  - Restart the containers
#   logs     - View logs
#   clean    - Stop containers and remove volumes
#   build    - Rebuild the Docker image

set -e

COMMAND=${1:-start}

case "$COMMAND" in
  start)
    echo "Starting blazor-net-app with Docker Compose..."
    docker-compose up -d
    echo ""
    echo "✓ Application started successfully!"
    echo ""
    echo "Access the application at:"
    echo "  - Web UI: http://localhost:5000"
    echo "  - Swagger API: http://localhost:5000/swagger"
    echo "  - MQTT Broker: localhost:1883"
    echo ""
    echo "View logs with: ./docker-dev.sh logs"
    echo "Stop with: ./docker-dev.sh stop"
    ;;
  
  stop)
    echo "Stopping containers..."
    docker-compose down
    echo "✓ Containers stopped"
    ;;
  
  restart)
    echo "Restarting containers..."
    docker-compose restart
    echo "✓ Containers restarted"
    ;;
  
  logs)
    echo "Viewing logs (Ctrl+C to exit)..."
    docker-compose logs -f
    ;;
  
  clean)
    echo "Stopping containers and removing volumes..."
    docker-compose down -v
    echo "✓ Cleanup complete"
    ;;
  
  build)
    echo "Rebuilding Docker image..."
    docker-compose build --no-cache
    echo "✓ Image rebuilt successfully"
    ;;
  
  *)
    echo "Usage: ./docker-dev.sh [command]"
    echo ""
    echo "Commands:"
    echo "  start    - Build and start the containers (default)"
    echo "  stop     - Stop the containers"
    echo "  restart  - Restart the containers"
    echo "  logs     - View logs"
    echo "  clean    - Stop containers and remove volumes"
    echo "  build    - Rebuild the Docker image"
    exit 1
    ;;
esac
