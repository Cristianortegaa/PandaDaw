using CSharpFunctionalExtensions;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Mappers;
using PandaBack.Repository;

namespace PandaBack.Services;

public class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _repository;
    private readonly IProductoRepository _productoRepository;

    public FavoritoService(IFavoritoRepository repository, IProductoRepository productoRepository)
    {
        _repository = repository;
        _productoRepository = productoRepository;
    }

    public async Task<Result<IEnumerable<FavoritoResponseDto>, PandaError>> GetUserFavoritosAsync(long userId)
    {
        var favoritos = await _repository.GetByUserIdAsync(userId);
        
        var response = favoritos.Select(f => f.ToDto());
        
        return Result.Success<IEnumerable<FavoritoResponseDto>, PandaError>(response);
    }

    public async Task<Result<FavoritoResponseDto, PandaError>> AddToFavoritosAsync(long userId, CreateFavoritoDto dto)
    {
        var producto = await _productoRepository.GetByIdAsync(dto.ProductoId);
        
        if (producto == null)
            return Result.Failure<FavoritoResponseDto, PandaError>(new NotFoundError("El producto no existe"));

        var existente = await _repository.GetByProductAndUserAsync(dto.ProductoId, userId);
        
        if (existente != null)
            return Result.Failure<FavoritoResponseDto, PandaError>(new BadRequestError("Este producto ya está en tu lista de favoritos"));

        var nuevoFavorito = new Favorito
        {
            UserId = userId,
            ProductoId = dto.ProductoId,
            CreatedAt = DateTime.UtcNow 
        };

        await _repository.AddAsync(nuevoFavorito);
        
        nuevoFavorito.Producto = producto; 

        return Result.Success<FavoritoResponseDto, PandaError>(nuevoFavorito.ToDto());
    }

    public async Task<UnitResult<PandaError>> RemoveFromFavoritosAsync(long id, long userId)
    {
        var favorito = await _repository.GetByIdAsync(id);

        if (favorito == null)
            return UnitResult.Failure<PandaError>(new NotFoundError("El favorito no fue encontrado"));

        await _repository.DeleteAsync(id);

        return UnitResult.Success<PandaError>();
    }
}