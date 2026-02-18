using System;

namespace DesignPatternChallenge
{
    // =========================================================
    // 1. Interfaces (O contrato comum para todos os gateways)
    // =========================================================
    public interface IPaymentValidator
    {
        bool ValidateCard(string cardNumber);
    }

    public interface IPaymentProcessor
    {
        string ProcessTransaction(decimal amount, string cardNumber);
    }

    public interface IPaymentLogger
    {
        void Log(string message);
    }

    // A Fábrica Abstrata: garante que as peças criadas sejam compatíveis entre si
    public interface IPaymentFactory
    {
        IPaymentValidator CreateValidator();
        IPaymentProcessor CreateProcessor();
        IPaymentLogger CreateLogger();
    }

    // =========================================================
    // 2. Implementações Concretas (PagSeguro, MP, Stripe)
    // =========================================================

    // --- Família PagSeguro ---
    public class PagSeguroValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber) => cardNumber.Length == 16;
    }

    public class PagSeguroProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"PagSeguro: Processando R$ {amount}...");
            return $"PAGSEG-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class PagSeguroLogger : IPaymentLogger
    {
        public void Log(string message) => Console.WriteLine($"[PagSeguro Log] {DateTime.Now}: {message}");
    }

    // Fábrica PagSeguro: Só entrega peças do PagSeguro
    public class PagSeguroFactory : IPaymentFactory
    {
        public IPaymentValidator CreateValidator() => new PagSeguroValidator();
        public IPaymentProcessor CreateProcessor() => new PagSeguroProcessor();
        public IPaymentLogger CreateLogger() => new PagSeguroLogger();
    }

    // --- Família MercadoPago ---
    public class MercadoPagoValidator : IPaymentValidator
    {
        // Regra específica: começa com 5
        public bool ValidateCard(string cardNumber) => cardNumber.Length == 16 && cardNumber.StartsWith("5");
    }

    public class MercadoPagoProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"MercadoPago: Processando R$ {amount}...");
            return $"MP-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class MercadoPagoLogger : IPaymentLogger
    {
        public void Log(string message) => Console.WriteLine($"[MercadoPago Log] {DateTime.Now}: {message}");
    }

    public class MercadoPagoFactory : IPaymentFactory
    {
        public IPaymentValidator CreateValidator() => new MercadoPagoValidator();
        public IPaymentProcessor CreateProcessor() => new MercadoPagoProcessor();
        public IPaymentLogger CreateLogger() => new MercadoPagoLogger();
    }

    // --- Família Stripe ---
    public class StripeValidator : IPaymentValidator
    {
        // Regra específica: começa com 4
        public bool ValidateCard(string cardNumber) => cardNumber.Length == 16 && cardNumber.StartsWith("4");
    }

    public class StripeProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"Stripe: Processando ${amount}...");
            return $"STRIPE-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class StripeLogger : IPaymentLogger
    {
        public void Log(string message) => Console.WriteLine($"[Stripe Log] {DateTime.Now}: {message}");
    }

    public class StripeFactory : IPaymentFactory
    {
        public IPaymentValidator CreateValidator() => new StripeValidator();
        public IPaymentProcessor CreateProcessor() => new StripeProcessor();
        public IPaymentLogger CreateLogger() => new StripeLogger();
    }

    // =========================================================
    // 3. Cliente (Desacoplado)
    // =========================================================
    public class PaymentService
    {
        private readonly IPaymentValidator _validator;
        private readonly IPaymentProcessor _processor;
        private readonly IPaymentLogger _logger;

        // Injeção de Dependência: Recebe a fábrica, não sabe qual gateway é.
        public PaymentService(IPaymentFactory factory)
        {
            _validator = factory.CreateValidator();
            _processor = factory.CreateProcessor();
            _logger = factory.CreateLogger();
        }

        public void ProcessPayment(decimal amount, string cardNumber)
        {
            if (!_validator.ValidateCard(cardNumber))
            {
                Console.WriteLine("Erro: Cartão inválido.");
                return;
            }

            var result = _processor.ProcessTransaction(amount, cardNumber);
            _logger.Log($"Sucesso: {result}");
        }
    }

    // =========================================================
    // 4. Execução
    // =========================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema Multi-Gateway (Abstract Factory) ===\n");

            // Exemplo 1: Usando PagSeguro
            // O Main decide qual fábrica usar (configuração)
            var service = new PaymentService(new PagSeguroFactory());
            Console.WriteLine("--- Teste PagSeguro ---");
            service.ProcessPayment(150.00m, "1234567890123456");

            Console.WriteLine();

            // Exemplo 2: Trocando para MercadoPago
            service = new PaymentService(new MercadoPagoFactory());
            Console.WriteLine("--- Teste MercadoPago ---");
            service.ProcessPayment(200.00m, "5234567890123456");
            
            Console.WriteLine();

            // Exemplo 3: Trocando para Stripe
            service = new PaymentService(new StripeFactory());
            Console.WriteLine("--- Teste Stripe ---");
            service.ProcessPayment(300.00m, "4234567890123456");
        }
    }
}