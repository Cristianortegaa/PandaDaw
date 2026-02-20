using PandaBack.Models;
using PandaBack.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using PandaBack.Data;

namespace PandaBack.Seeders;

public class DataSeeder
{
    public static void Initialize(PandaDbContext context)
    {
        if (!context.Productos.Any())
        {
            var productosMarca = new List<Producto>
            {
                new Producto
                {
                    Nombre = "iPhone 15 Pro Max 256GB",
                    Descripcion = "Cuerpo de titanio aerospacial, chip A17 Pro y el sistema de cámaras más avanzado de Apple.",
                    Precio = 1469.00m,
                    Stock = 20,
                    Imagen = "https://acdn.mitiendanube.com/stores/001/174/820/products/iphone-15-pro-max-titanio-natural1-7ab3fc0db0c9c7b50816952994406248-640-0.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Smartphones
                },
                new Producto
                {
                    Nombre = "Samsung Galaxy S24 Ultra",
                    Descripcion = "Inteligencia Artificial integrada, cámara de 200MP y S Pen incorporado. La bestia de Android.",
                    Precio = 1479.00m,
                    Stock = 15,
                    Imagen = "https://m.media-amazon.com/images/I/71WcjsOVOmL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Smartphones
                },
                new Producto
                {
                    Nombre = "Google Pixel 8 Pro",
                    Descripcion = "El smartphone de Google con la mejor cámara computacional y Android puro con 7 años de actualizaciones.",
                    Precio = 1099.00m,
                    Stock = 18,
                    Imagen = "https://m.media-amazon.com/images/I/71R2hE9ZqUL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Smartphones
                },
                new Producto
                {
                    Nombre = "Xiaomi 14 Pro",
                    Descripcion = "Cámaras co-desarrolladas con Leica, carga ultrarrápida de 120W y pantalla AMOLED ultrabrillante.",
                    Precio = 999.00m,
                    Stock = 25,
                    Imagen = "https://m.media-amazon.com/images/I/61NlU2+Z2uL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Smartphones
                },
                new Producto
                {
                    Nombre = "AirPods Pro (2.ª generación)",
                    Descripcion = "Cancelación activa de ruido el doble de potente, audio espacial personalizado y estuche MagSafe.",
                    Precio = 279.00m,
                    Stock = 50,
                    Imagen = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83?wid=1144&hei=1144&fmt=jpeg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Audio
                },
                new Producto
                {
                    Nombre = "Sony WH-1000XM5",
                    Descripcion = "Auriculares de diadema inalámbricos con la mejor cancelación de ruido de la industria y 30h de batería.",
                    Precio = 349.00m,
                    Stock = 25,
                    Imagen = "https://m.media-amazon.com/images/I/51aXvjzcukL._AC_SL1200_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Audio
                },
                new Producto
                {
                    Nombre = "Bose QuietComfort Ultra",
                    Descripcion = "Audio inmersivo revolucionario, diseño premium y una cancelación de ruido legendaria.",
                    Precio = 499.00m,
                    Stock = 15,
                    Imagen = "https://m.media-amazon.com/images/I/51wXwN3tW-L._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Audio
                },
                new Producto
                {
                    Nombre = "Altavoz JBL Flip 6",
                    Descripcion = "Altavoz Bluetooth portátil, resistente al agua y al polvo, con hasta 12 horas de reproducción continua.",
                    Precio = 129.00m,
                    Stock = 40,
                    Imagen = "https://m.media-amazon.com/images/I/61bX2AoGjIG._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Audio
                },
                new Producto
                {
                    Nombre = "MacBook Pro 16\" M3 Max",
                    Descripcion = "El portátil definitivo para profesionales. Chip M3 Max, 36GB de RAM y pantalla Liquid Retina XDR.",
                    Precio = 4049.00m,
                    Stock = 5,
                    Imagen = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/mbp16-spaceblack-select-202310?wid=904&hei=840&fmt=jpeg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Laptops
                },
                new Producto
                {
                    Nombre = "ASUS ROG Zephyrus G14",
                    Descripcion = "Portátil ultraligero para gaming con pantalla OLED a 120Hz, procesador AMD Ryzen 9 y RTX 4070.",
                    Precio = 1999.00m,
                    Stock = 8,
                    Imagen = "https://m.media-amazon.com/images/I/71YtVq2R-JL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Laptops
                },
                new Producto
                {
                    Nombre = "Dell XPS 15",
                    Descripcion = "Pantalla InfinityEdge OLED 3.5K, procesador Intel Core i9 de 13ª generación y diseño de aluminio mecanizado.",
                    Precio = 2299.00m,
                    Stock = 10,
                    Imagen = "https://m.media-amazon.com/images/I/61pBvlYFnwL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Laptops
                },
                new Producto
                {
                    Nombre = "MacBook Air 15\" M3",
                    Descripcion = "El portátil de 15 pulgadas más fino del mundo. Increíblemente ligero, potente y con 18h de autonomía.",
                    Precio = 1599.00m,
                    Stock = 20,
                    Imagen = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/mba15-midnight-select-202402?wid=904&hei=840&fmt=jpeg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Laptops
                },
                new Producto
                {
                    Nombre = "Consola PlayStation 5 (Slim)",
                    Descripcion = "Juego a 4K, memoria SSD ultrarrápida, gatillos adaptativos y audio 3D. La nueva generación.",
                    Precio = 549.99m,
                    Stock = 30,
                    Imagen = "https://m.media-amazon.com/images/I/51rJb1J2gPL._AC_SL1200_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Gaming
                },
                new Producto
                {
                    Nombre = "Nintendo Switch Modelo OLED",
                    Descripcion = "Pantalla OLED de 7 pulgadas con colores intensos y contraste alto para jugar donde quieras.",
                    Precio = 349.00m,
                    Stock = 40,
                    Imagen = "https://m.media-amazon.com/images/I/61-PblYntsL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Gaming
                },
                new Producto
                {
                    Nombre = "Xbox Series X",
                    Descripcion = "La Xbox más rápida y potente de la historia. 12 teraflops de potencia de procesamiento y juego 4K real.",
                    Precio = 499.00m,
                    Stock = 25,
                    Imagen = "https://m.media-amazon.com/images/I/51ojzJk77qL._AC_SL1000_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Gaming
                },
                new Producto
                {
                    Nombre = "Valve Steam Deck OLED",
                    Descripcion = "Tu biblioteca de PC en tus manos. Pantalla HDR OLED brillante, mayor duración de batería y Wi-Fi 6E.",
                    Precio = 569.00m,
                    Stock = 12,
                    Imagen = "https://m.media-amazon.com/images/I/51kP3jD-m5L._AC_SL1200_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Gaming
                },
                new Producto
                {
                    Nombre = "Sony Alpha 7 IV (Cámara Mirrorless)",
                    Descripcion = "Sensor Full-Frame de 33 MP, grabación 4K 60p y el mejor enfoque automático en tiempo real.",
                    Precio = 2799.00m,
                    Stock = 4,
                    Imagen = "https://m.media-amazon.com/images/I/71wE8kH++NL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Imagen
                },
                new Producto
                {
                    Nombre = "Smart TV LG OLED 65\" Serie C3",
                    Descripcion = "Televisor 4K con píxeles autoluminiscentes, procesador a9 Gen6 y optimizado para gaming a 120Hz.",
                    Precio = 1699.00m,
                    Stock = 12,
                    Imagen = "https://m.media-amazon.com/images/I/81e5W4WbQjL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Imagen
                },
                new Producto
                {
                    Nombre = "Dron DJI Mini 4 Pro",
                    Descripcion = "Dron ligero (menos de 250g) con detección de obstáculos omnidireccional y vídeo HDR en 4K/60 fps.",
                    Precio = 999.00m,
                    Stock = 8,
                    Imagen = "https://m.media-amazon.com/images/I/61NlU2+Z2uL._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Imagen
                },
                new Producto
                {
                    Nombre = "Cámara de Acción GoPro HERO12 Black",
                    Descripcion = "Increíble calidad de imagen, estabilización de vídeo HyperSmooth 6.0 mejorada y autonomía excepcional.",
                    Precio = 449.00m,
                    Stock = 30,
                    Imagen = "https://m.media-amazon.com/images/I/61fWqjL8w+L._AC_SL1500_.jpg",
                    FechaAlta = DateTime.UtcNow,
                    IsDeleted = false,
                    Category = Categoria.Imagen
                }
            };

            context.Productos.AddRange(productosMarca);
            context.SaveChanges();
        }
    }
}