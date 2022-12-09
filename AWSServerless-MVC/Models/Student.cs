﻿using Amazon.DynamoDBv2.DataModel;

namespace AWSServerless_MVC.Models
{
    [DynamoDBTable("students")]
    public class Student
    {
        [DynamoDBHashKey("Id")]
        public int? Id { get; set; }
        [DynamoDBProperty("first_name")]
        public string? FirstName { get; set; }
        [DynamoDBProperty("last_name")]
        public string? LastName { get; set; }
        [DynamoDBProperty("class")]
        public int Class { get; set; }
        [DynamoDBProperty("country")]
        public string? Country { get; set; }
    }
}
