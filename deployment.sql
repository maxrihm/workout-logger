CREATE TABLE `my_exercises` (
   `exercise_id` int NOT NULL AUTO_INCREMENT,
   `exercise_name` varchar(135) DEFAULT NULL,
   `exercise_weight` double(8,4) DEFAULT NULL,
   `exercise_set` int DEFAULT NULL,
   `exercise_reps` int DEFAULT NULL,
   `exercise_date` date DEFAULT (now()),
   `total_weight` double DEFAULT '0',
   `exercise_instance` varchar(255) DEFAULT NULL,
   PRIMARY KEY (`exercise_id`)
 ) ENGINE=InnoDB AUTO_INCREMENT=2113 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci