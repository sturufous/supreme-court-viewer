

## Management Script

The `manage.sh` script wraps the Docker process in easy to use commands.

To get full usage information on the script run:
```
./manage.sh -h
```
  
## Building the Images

The first thing you'll need to do is build the Docker images. 

To build the images run:
```
./manage.sh build
```


## Starting the Project

To start the project run:
```
./manage.sh start
```

This will start the project interactively; with all of the logs being written to the command line.  Press `Ctrl-C` to shut down the services from the same shell window.

## Stopping the Project

To stop the project run:
```
./manage.sh stop
```

This will shut down and clean up all of the containers in the project.  This is a non-destructive process.  The containers are not deleted so they will be reused the next time you run start.

Since the services are started interactively, you will have to issue this command from another shell window.  This command can also be run after shutting down the services using the `Ctrl-C` method to clean up any services that may not have shutdown correctly.

## Using the Application

* The main UI is exposed at; http://localhost:8080/

