using Application.DTOs.Favorites;
using Application.DTOs.Profile;
using Domain.Repositories;
using Domain.Services;

namespace Application.UseCases.Profile;

public class GetProfileUseCase(
    IUsuarioRepository usuarioRepo,
    IFavoritoRepository favoritoRepo)
{
    public async Task<ProfileResponse?> ExecuteAsync(int usuarioId)
    {
        var usuario = await usuarioRepo.GetByIdAsync(usuarioId);
        if (usuario == null) return null;

        var favs = await favoritoRepo.GetByUsuarioAsync(usuarioId);
        var favDtos = favs.Select(f => new FavoriteDto(f.Id, f.TmdbMovieId, f.Titulo, f.PosterPath, f.CriadoEm));

        return new ProfileResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            FotoUrl = usuario.FotoUrl,
            Bio = usuario.Bio,
            CriadoEm = usuario.CriadoEm,
            Favoritos = favDtos
        };
    }
}

public class UpdateProfileUseCase(IUsuarioRepository usuarioRepo)
{
    public async Task ExecuteAsync(int loggedInUserId, UpdateProfileRequest req)
    {
        // Validação anti-IDOR explícita: Se o cliente forneceu um ID e esse ID difere do usuário logado no token
        if (req.TargetUserId.HasValue && req.TargetUserId.Value != loggedInUserId)
        {
            throw new UnauthorizedAccessException("Acesso negado: Você não tem permissão para editar o perfil de outro usuário.");
        }

        await usuarioRepo.UpdateProfileAsync(loggedInUserId, req.Bio);
    }
}

public class UploadProfilePhotoUseCase(
    IUsuarioRepository usuarioRepo,
    IStorageService storageService)
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB

    public async Task<UploadPhotoResponse> ExecuteAsync(
        int loggedInUserId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileLength)
    {
        if (fileLength > MaxFileSize)
        {
            throw new ArgumentException("O arquivo de imagem excede o tamanho máximo permitido de 2 MB.");
        }

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException("Tipo de arquivo inválido. Permitido apenas imagens (.jpg, .jpeg, .png, .webp).");
        }

        var usuario = await usuarioRepo.GetByIdAsync(loggedInUserId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        // Se o usuário já tinha foto anterior, remove do storage para evitar acúmulo de arquivos órfãos
        if (!string.IsNullOrWhiteSpace(usuario.FotoChave))
        {
            await storageService.DeleteFileAsync(usuario.FotoChave);
        }

        var objectKey = $"avatars/{loggedInUserId}-{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        await storageService.UploadFileAsync(fileStream, objectKey, contentType);

        var publicUrl = storageService.GetPublicUrl(objectKey);
        await usuarioRepo.UpdateFotoAsync(loggedInUserId, objectKey, publicUrl);

        return new UploadPhotoResponse
        {
            FotoUrl = publicUrl,
            Mensagem = "Foto de perfil atualizada com sucesso no MinIO."
        };
    }
}
