## WorkoutLog Application
This is a WPF desktop application written in C# that allows users to track their workouts and progress using the progressive overload method. The application uses MySQL for the database and includes a timer, support for additional sets, and dropsets.

### Progressive Overload
The application uses the following formula to calculate progressive overload and help users increase their volume during their workouts:
> Personal Record Volume = (Current Personal Record Volume - Current Total Volume + 1) / (Current Set Weight * (Total Sets - Current Set Number))

The formula shows users how many reps they should perform to increase the volume they had achieved the last time they did this exercise. It displays both numbers of reps to beat for the last time and for the PR. Users will see two badges if they beat their current PR or if they did more volume from the last time to track their progressive overload.

### CRUD Operations and Logs
The application supports CRUD operations to fetch and write data from the database. Users can access their workout logs from any time range.

### Deployment
There is a file provided to deploy the database. The application reads data from a text file and creates a grid with inputs and outputs. From the text file, it takes the count of sets, count of reps, name of the exercise, and time to relax between sets.

### Features
* Timer: It counts time between sets and plays sounds.
* Additional set: If users want to get more volume from their exercises, they can use this feature.
* Dropsets: If users want to make their exercise with a lower or higher weight altogether, they can use this feature.

### Usage
The following GIF files demonstrate how to use the features in the application:
* **Progressive overload**

![po1](https://user-images.githubusercontent.com/79306299/229292528-93c1086a-0082-405d-983f-88b1382d9cba.gif)

* **Adding an additional set** 

To activate the additional set feature, you should click on the field that shows your total volume from the last time in the bottom right. This will bring up an additional field where you can add a set.

![as1](https://user-images.githubusercontent.com/79306299/229292544-a438a91a-3814-4ec8-825b-9d3d87432ded.gif)

* **Using dropsets**

To activate the drop set feature, you should double click on the desired set you want to convert into a drop set.

![ds1](https://user-images.githubusercontent.com/79306299/229292548-05b3072a-76fa-4df8-b777-6b6e38485fcc.gif)

### Note
MVVM pattern will be implemented in the future to get rid of big fat files.
