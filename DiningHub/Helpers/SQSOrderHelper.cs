using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiningHub.Areas.Identity.Data;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;


namespace DiningHub.Helpers
{
    public class SqsBackgroundService : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly AWSSettings _awsSettings;
        private readonly ILogger<SqsBackgroundService> _logger;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SqsBackgroundService(IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient, IOptions<AWSSettings> awsSettings, ILogger<SqsBackgroundService> logger, UserManager<DiningHubUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            _awsSettings = awsSettings.Value;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _awsSettings.SqsQueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    try
                    {
                        var orderDetails = JsonSerializer.Deserialize<OrderDetailsMessage>(message.Body);

                        // Send notification to the customer
                        var customerMessage = new StringBuilder();
                        customerMessage.AppendLine($"Dear {orderDetails.UserName}, your order {orderDetails.OrderId} has been placed successfully.");
                        customerMessage.AppendLine("Order Details:");
                        foreach (var item in orderDetails.OrderItems)
                        {
                            customerMessage.AppendLine($"- {item.MenuItemName} x {item.Quantity} @ {item.Price:C}");
                        }
                        customerMessage.AppendLine($"Total Amount: {orderDetails.TotalAmount:C}");

                        var customerRequest = new PublishRequest
                        {
                            TopicArn = _awsSettings.SnsTopicArn,
                            Message = customerMessage.ToString()
                        };
                        await _snsClient.PublishAsync(customerRequest);

                        // Send notification to all staff
                        var staffMessage = new StringBuilder();
                        staffMessage.AppendLine($"Order {orderDetails.OrderId} has been placed by {orderDetails.UserName}.");
                        staffMessage.AppendLine("Order Details:");
                        foreach (var item in orderDetails.OrderItems)
                        {
                            staffMessage.AppendLine($"- {item.MenuItemName} x {item.Quantity} @ {item.Price:C}");
                        }
                        staffMessage.AppendLine($"Total Amount: {orderDetails.TotalAmount:C}");

                        var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");
                        foreach (var staffUser in staffUsers)
                        {
                            var staffRequest = new PublishRequest
                            {
                                TopicArn = _awsSettings.SnsTopicArn,
                                Message = staffMessage.ToString()
                            };
                            await _snsClient.PublishAsync(staffRequest);
                        }

                        _logger.LogInformation($"Processed order {orderDetails.OrderId} and sent notifications.");

                        // Delete message from SQS
                        var deleteMessageRequest = new DeleteMessageRequest
                        {
                            QueueUrl = _awsSettings.SqsQueueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        };
                        await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message {message.MessageId}");
                    }
                }

                await Task.Delay(5000, stoppingToken); // Poll every 5 seconds
            }
        }

        private class OrderDetailsMessage
        {
            public int OrderId { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public List<OrderItemDetails> OrderItems { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private class OrderItemDetails
        {
            public string MenuItemName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
    }

}
