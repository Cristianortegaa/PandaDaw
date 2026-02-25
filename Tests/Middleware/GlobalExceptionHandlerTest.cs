using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Middleware;

namespace Tests.Middleware
{
    public class GlobalExceptionHandlerTest
    {
        private Mock<ILogger<GlobalExceptionHandler>> _loggerFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _loggerFalso = new Mock<ILogger<GlobalExceptionHandler>>();
        }

        // ==========================================
        // HELPER: Ejecutar el middleware con una excepción
        // ==========================================

        private async Task<(int statusCode, JsonDocument body)> EjecutarMiddlewareConExcepcion(Exception excepcion)
        {
            var middleware = new GlobalExceptionHandler(
                next: _ => throw excepcion,
                logger: _loggerFalso.Object
            );

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var json = JsonDocument.Parse(responseBody);

            return (context.Response.StatusCode, json);
        }

        // ==========================================
        // 1. SIN EXCEPCIÓN - DEBE PASAR SIN PROBLEMAS
        // ==========================================

        [Test]
        public async Task InvokeAsync_SinExcepcion_DebeEjecutarNextSinProblemas()
        {
            // PREPARAR
            var nextInvocado = false;
            var middleware = new GlobalExceptionHandler(
                next: _ =>
                {
                    nextInvocado = true;
                    return Task.CompletedTask;
                },
                logger: _loggerFalso.Object
            );

            var context = new DefaultHttpContext();

            // ACTUAR
            await middleware.InvokeAsync(context);

            // COMPROBAR
            Assert.That(nextInvocado, Is.True);
        }

        // ==========================================
        // 2. UnauthorizedAccessException → 401
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConUnauthorizedAccessException_DebeDevolver401()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new UnauthorizedAccessException("No autorizado")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(401));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("No autorizado"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("UnauthorizedError"));
        }

        // ==========================================
        // 3. InvalidOperationException → 400
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConInvalidOperationException_DebeDevolver400()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new InvalidOperationException("Operación no válida")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(400));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Operación no válida"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("ValidationError"));
        }

        // ==========================================
        // 4. ArgumentException → 400
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConArgumentException_DebeDevolver400()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new ArgumentException("Argumento inválido")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(400));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Argumento inválido"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("ValidationError"));
        }

        // ==========================================
        // 5. KeyNotFoundException → 404
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConKeyNotFoundException_DebeDevolver404()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new KeyNotFoundException("Recurso no encontrado")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(404));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Recurso no encontrado"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("NotFoundError"));
        }

        // ==========================================
        // 6. DbUpdateException → 409
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConDbUpdateException_DebeDevolver409()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new DbUpdateException("Error de BD")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(409));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Error al actualizar la base de datos"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("ConflictError"));
        }

        // ==========================================
        // 7. TimeoutException → 408
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConTimeoutException_DebeDevolver408()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new TimeoutException("Timeout")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(408));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Tiempo de espera agotado"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("TimeoutError"));
        }

        // ==========================================
        // 8. Exception genérica → 500
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConExcepcionGenerica_DebeDevolver500()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new Exception("Error inesperado")
            );

            // COMPROBAR
            Assert.That(statusCode, Is.EqualTo(500));
            Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Ha ocurrido un error interno"));
            Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("InternalError"));
        }

        // ==========================================
        // 9. VERIFICAR ESTRUCTURA DE RESPUESTA JSON
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConExcepcion_DebeIncluirCamposEsperados()
        {
            // ACTUAR
            var (statusCode, json) = await EjecutarMiddlewareConExcepcion(
                new Exception("Test")
            );

            // COMPROBAR
            var root = json.RootElement;
            Assert.That(root.TryGetProperty("errorId", out _), Is.True);
            Assert.That(root.TryGetProperty("message", out _), Is.True);
            Assert.That(root.TryGetProperty("errorType", out _), Is.True);
            Assert.That(root.TryGetProperty("timestamp", out _), Is.True);
            Assert.That(root.TryGetProperty("path", out _), Is.True);
            Assert.That(root.TryGetProperty("method", out _), Is.True);
        }

        // ==========================================
        // 10. VERIFICAR QUE SE USA application/json
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConExcepcion_DebeUsarContentTypeJson()
        {
            // PREPARAR
            var middleware = new GlobalExceptionHandler(
                next: _ => throw new Exception("Error"),
                logger: _loggerFalso.Object
            );

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // ACTUAR
            await middleware.InvokeAsync(context);

            // COMPROBAR
            Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));
        }

        // ==========================================
        // 11. VERIFICAR QUE SE LOGUEA LA EXCEPCIÓN
        // ==========================================

        [Test]
        public async Task InvokeAsync_ConExcepcion_DebeLoguearError()
        {
            // PREPARAR
            var middleware = new GlobalExceptionHandler(
                next: _ => throw new Exception("Error de prueba"),
                logger: _loggerFalso.Object
            );

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // ACTUAR
            await middleware.InvokeAsync(context);

            // COMPROBAR
            _loggerFalso.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
