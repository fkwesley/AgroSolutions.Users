using Application.DTO.Common;
using Application.DTO.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
    /// <summary>
    /// Helper para gerar links HATEOAS consistentes.
    /// 
    /// ?? OBJETIVO:
    /// Centralizar a lógica de criação de links HATEOAS,
    /// garantindo URLs corretas e consistentes.
    /// 
    /// ?? USO:
    /// var links = HateoasHelper.CreateUserLinks(urlHelper, userId, version);
    /// userResponse.Links = links;
    /// </summary>
    public static class HateoasHelper
    {
        /// <summary>
        /// Cria links HATEOAS para um usuário específico.
        /// </summary>
        /// <param name="urlHelper">Helper para gerar URLs</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="version">Versão da API (ex: "1.0")</param>
        /// <param name="isActive">Status atual do usuário (para links condicionais)</param>
        /// <returns>Lista de links HATEOAS</returns>
        public static List<Link> CreateUserLinks(IUrlHelper urlHelper, string userId, string version, bool? isActive = null)
        {
            var links = new List<Link>
            {
                // Self - Link para o próprio recurso
                new Link(
                    href: urlHelper.Link("GetUserById", new { userId = userId, version }) ?? string.Empty,
                    rel: "self",
                    method: "GET"
                ),

                // Update - Link para atualizar o usuário
                new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "update",
                    method: "PUT"
                ),

                // Delete - Link para deletar
                new Link(
                    href: urlHelper.Link("DeleteUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ),

                // All - Link para lista de todos os usuários
                new Link(
                    href: urlHelper.Link("GetAllUsers", new { version }) ?? string.Empty,
                    rel: "all",
                    method: "GET"
                )
            };

            // Links condicionais baseados no status
            if (isActive == false)
            {
                links.Add(new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "activate",
                    method: "PUT"
                ));
            }

            if (isActive == true)
            {
                links.Add(new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "deactivate",
                    method: "PUT"
                ));
            }

            return links;
        }

        /// <summary>
        /// Adiciona links HATEOAS a um usuário.
        /// </summary>
        public static void AddLinksToUser(UserResponse user, IUrlHelper urlHelper, string version)
        {
            user.Links = CreateUserLinks(urlHelper, user.UserId, version, user.IsActive);
        }

        /// <summary>
        /// Adiciona links HATEOAS a uma coleção de usuários.
        /// </summary>
        public static void AddLinksToUsers(IEnumerable<UserResponse> users, IUrlHelper urlHelper, string version)
        {
            foreach (var user in users)
            {
                AddLinksToUser(user, urlHelper, version);
            }
        }

        /// <summary>
        /// Gera links HATEOAS para Health Check endpoints.
        /// </summary>
        /// <param name="httpContext">HTTP Context para gerar URLs</param>
        /// <param name="version">Versão da API</param>
        /// <returns>Lista de links HATEOAS para health endpoints</returns>
        public static List<Link> GenerateHealthLinks(HttpContext httpContext, string version)
        {
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var links = new List<Link>
            {
                // Self - Health check completo
                new Link(
                    href: $"{baseUrl}/v{version}/health",
                    rel: "self",
                    method: "GET"
                ),

                // Users - Link para endpoints principais
                new Link(
                    href: $"{baseUrl}/api/v{version}/users",
                    rel: "users",
                    method: "GET"
                ),

                // Swagger Documentation
                new Link(
                    href: $"{baseUrl}/swagger/index.html",
                    rel: "documentation",
                    method: "GET"
                )
            };

            return links;
        }
    }
}


