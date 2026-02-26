using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Models;
using PandaBack.Services;
using PandaDawRazor.Services;


namespace PandaDawRazor.Pages;

[RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
[RequestSizeLimit(52428800)]
public class AdminPanelModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly NotificacionService _notificacionService;
    private readonly IWebHostEnvironment _env;

    public AdminPanelModel(IProductoService productoService, NotificacionService notificacionService, IWebHostEnvironment env)
    {
        _productoService = productoService;
        _notificacionService = notificacionService;
        _env = env;
    }

    public List<Producto> Productos { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public ProductoInputModel ProductoInput { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filtro { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool MostrarEliminados { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verificar que el usuario sea admin
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        if (!Enum.TryParse<Categoria>(ProductoInput.Categoria, out var categoria))
        {
            ErrorMessage = "Categoría no válida";
            await CargarProductos();
            return Page();
        }

        // Procesar imagen subida
        string? imagenPath = null;
        if (ProductoInput.ImagenArchivo != null && ProductoInput.ImagenArchivo.Length > 0)
        {
            imagenPath = await GuardarImagenAsync(ProductoInput.ImagenArchivo);
        }

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = imagenPath
        };

        var result = await _productoService.CreateProductoAsync(producto);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto creado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "success",
                Titulo = "Producto creado",
                Mensaje = $"Se ha creado el producto: {producto.Nombre}",
                Icono = "fa-solid fa-plus-circle"
            });
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostEditarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        if (!Enum.TryParse<Categoria>(ProductoInput.Categoria, out var categoria))
        {
            ErrorMessage = "Categoría no válida";
            await CargarProductos();
            return Page();
        }

        // Procesar imagen subida (si hay nueva)
        string? imagenPath = ProductoInput.ImagenActual;
        if (ProductoInput.ImagenArchivo != null && ProductoInput.ImagenArchivo.Length > 0)
        {
            try
            {
                imagenPath = await GuardarImagenAsync(ProductoInput.ImagenArchivo);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al subir la imagen: {ex.Message}";
                await CargarProductos();
                return Page();
            }
        }

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = imagenPath
        };

        var result = await _productoService.UpdateProductoAsync(id, producto);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto actualizado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "info",
                Titulo = "Producto actualizado",
                Mensaje = $"Se ha actualizado: {producto.Nombre}",
                Icono = "fa-solid fa-pen-to-square"
            });
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

var result = await _productoService.DeleteProductoAsync(id);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto eliminado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "error",
                Titulo = "Producto eliminado",
                Mensaje = "Un producto ha sido eliminado del catálogo",
                Icono = "fa-solid fa-trash"
            });
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostRestaurarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        // Obtener el producto y restaurarlo
        var productoResult = await _productoService.GetProductoByIdAsync(id);
        if (productoResult.IsSuccess)
        {
            var producto = productoResult.Value;
            producto.IsDeleted = false;
var result = await _productoService.UpdateProductoAsync(id, producto);
            
            if (result.IsSuccess)
            {
                SuccessMessage = "Producto restaurado correctamente";
                _notificacionService.EnviarATodos(new Notificacion
                {
                    Tipo = "success",
                    Titulo = "Producto restaurado",
                    Mensaje = $"El producto {producto.Nombre} ha sido restaurado",
                    Icono = "fa-solid fa-trash-arrow-up"
                });
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        else
        {
            ErrorMessage = productoResult.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    private async Task CargarProductos()
    {
        var result = await _productoService.GetAllProductosAsync();
        if (result.IsSuccess)
        {
            Productos = result.Value.ToList();

            // Filtrar por texto si hay filtro
            if (!string.IsNullOrEmpty(Filtro))
            {
                Productos = Productos
                    .Where(p => p.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrar eliminados según opción
            if (!MostrarEliminados)
            {
                Productos = Productos.Where(p => !p.IsDeleted).ToList();
            }

            // Ordenar por fecha de alta descendente
            Productos = Productos.OrderByDescending(p => p.FechaAlta).ToList();
        }
    }

    /// <summary>
    /// Guarda el archivo de imagen en wwwroot/img/productos/ y devuelve la ruta relativa.
    /// </summary>
    private async Task<string> GuardarImagenAsync(IFormFile archivo)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "img", "productos");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(uploadsDir, nombreArchivo);

        await using var stream = new FileStream(rutaCompleta, FileMode.Create);
        await archivo.CopyToAsync(stream);

        return $"/img/productos/{nombreArchivo}";
    }

    public class ProductoInputModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public IFormFile? ImagenArchivo { get; set; }
        public string? ImagenActual { get; set; }
    }
}
