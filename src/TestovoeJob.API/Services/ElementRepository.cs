using Dapper;
using Npgsql;
using TestovoeJob.API.Models;

namespace TestovoeJob.API.Services;

public sealed class ElementRepository
{
    private const string InsertSql = """
        INSERT INTO elements (attribute_value, html_code)
        VALUES (@AttributeValue, @HtmlCode);
        """;

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<ElementRepository> _logger;

    public ElementRepository(NpgsqlDataSource dataSource, ILogger<ElementRepository> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }
    public async Task InsertAllAsync(
        IReadOnlyList<ElementRecord> elements,
        CancellationToken cancellationToken = default)
    {
        if (elements.Count == 0)
        {
            return;
        }

        object[] parameters = [.. elements.Select(static element => new
        {
            AttributeValue = element.AttributeValue,
            HtmlCode = element.HtmlCode
        })];

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                InsertSql,
                parameters,
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not ProcessingException && exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Не удалось сохранить элементы в БД. Количество элементов: {ElementCount}. Транзакция откатена.",
                elements.Count);

            throw new ProcessingException(
                ProcessingErrorCodes.DatabaseError,
                $"Ошибка сохранения элементов в базу данных: {exception.Message}",
                exception);
        }
    }
}
