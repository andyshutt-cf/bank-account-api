#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: $0 <port_number>"
    echo "Example: $0 8080"
    exit 1
fi

PORT=$1

echo "Checking port $PORT..."

PID=$(lsof -ti :$PORT)

if [ -z "$PID" ]; then
    echo "Port $PORT is free - no process found."
    exit 0
fi

echo "Found process(es) using port $PORT: $PID"

for pid in $PID; do
    PROCESS_INFO=$(ps -p $pid -o comm=)
    echo "  PID $pid: $PROCESS_INFO"
done

read -p "Kill these process(es)? (y/n) " -n 1 -r
echo

if [[ $REPLY =~ ^[Yy]$ ]]; then
    for pid in $PID; do
        kill $pid
        if [ $? -eq 0 ]; then
            echo "Killed process $pid"
        else
            echo "Failed to kill process $pid"
        fi
    done
    
    sleep 1
    
    STILL_RUNNING=$(lsof -ti :$PORT)
    if [ -z "$STILL_RUNNING" ]; then
        echo "Port $PORT is now free."
    else
        echo "Warning: Port $PORT is still in use. Try with sudo or use kill -9"
    fi
else
    echo "Aborted - no processes killed."
    exit 1
fi
