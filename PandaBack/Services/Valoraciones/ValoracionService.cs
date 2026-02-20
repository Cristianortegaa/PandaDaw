using CSharpFunctionalExtensions;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;
using PandaBack.Mappers;
using PandaBack.Models;
using PandaBack.Repositories;

namespace PandaBack.Services;

public class ValoracionService : IValoracionService
{
    private readonly IValoracionRepository _valoracionRepository;
    private readonly IProductoRepository _productoRepository;

    public ValoracionService(IValoracionRepository valoracionRepository, IProductoRepository productoRepository)
    {
        _valoracionRepository = valoracionRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByProductoAsync(long productoId)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId);

        if (producto == null)
            return Result.Failure<IEnumerable<ValoracionResponseDto>, PandaError>(new NotFoundError($"Producto {productoId} no encontrado"));

        var valoraciones = await _valoracionRepository.GetByProductoIdAsync(productoId);

        var response = valoraciones.Select(v => v.ToDto());

        return Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(response);
    }

    public async Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByUserAsync(long userId)
    {
        var valoraciones = await _valoracionRepository.GetByUserIdAsync(userId);

        var response = valoraciones.Select(v => v.ToDto());

        return Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(response);
    }

    public async Task<Result<ValoracionResponseDto, PandaError>> CreateValoracionAsync(long userId, CreateValoracionDto dto)
    {
        var producto = await _productoRepository.GetByIdAsync(dto.ProductoId);

        if (producto == null)
            return Result.Failure<ValoracionResponseDto, PandaError>(new NotFoundError("El producto no existe"));

        var existente = await _valoracionRepository.GetByProductoAndUserAsync(dto.ProductoId, userId);

        if (existente != null)
            return Result.Failure<ValoracionResponseDto, PandaError>(new ConflictError("Ya has valorado este producto"));

        var valoracion = dto.ToModel(userId);

        await _valoracionRepository.AddAsync(valoracion);

        valoracion.Producto = producto;

        return Result.Success<ValoracionResponseDto, PandaError>(valoracion.ToDto());
    }

    public async Task<Result<ValoracionResponseDto, PandaError>> UpdateValoracionAsync(long id, long userId, CreateValoracionDto dto)
    {
        var valoracion = await _valoracionRepository.GetByIdAsync(id);

        if (valoracion == null)
            return Result.Failure<ValoracionResponseDto, PandaError>(new NotFoundError("Valoración no encontrada"));

        if (valoracion.UserId != userId)
            return Result.Failure<ValoracionResponseDto, PandaError>(new OperacionNoPermitidaError("No puedes modificar una valoración de otro usuario"));

        valoracion.Estrellas = dto.Estrellas;
        valoracion.Resena = dto.Resena;

        await _valoracionRepository.UpdateAsync(valoracion);

        return Result.Success<ValoracionResponseDto, PandaError>(valoracion.ToDto());
    }

    public async Task<UnitResult<PandaError>> DeleteValoracionAsync(long id, long userId)
    {
        var valoracion = await _valoracionRepository.GetByIdAsync(id);

        if (valoracion == null)
            return UnitResult.Failure<PandaError>(new NotFoundError("Valoración no encontrada"));

        if (valoracion.UserId != userId)
            return UnitResult.Failure<PandaError>(new OperacionNoPermitidaError("No puedes eliminar una valoración de otro usuario"));

        await _valoracionRepository.DeleteAsync(id);

        return UnitResult.Success<PandaError>();
    }
}

