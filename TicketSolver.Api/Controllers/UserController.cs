using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.User.Interfaces;

namespace TicketSolver.Api.Controllers;

// TODO: Verificar ser o usuário que está enviando a requisição tem permissão para editar ou remover tal usuário
// TODO: Filtrar registros da mesma empresa do usuário. É possível acessar o usuário através de AuthenticatedUser, definido na ShellController
public class UsersController(IUsersRepository usersRepository) : ShellController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Users>>> GetUsers(CancellationToken cancellationToken)
    {
        return await usersRepository.GetAll().Take(10).ToListAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Users>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetById(cancellationToken, id);
        if (user == null)
            return NotFound();

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<Users>> PostUser(Users users, CancellationToken cancellationToken)
    {
        await usersRepository.InsertAsync(cancellationToken, users);
        return CreatedAtAction(nameof(GetUser), new { id = users.Id }, users);
    }

    // TODO: Verificar ser o usuário que está enviando a requisição tem permissão para editar ou remover tal usuário
    // [HttpPut("{id}")]
    // public async Task<IActionResult> PutUser(int id, Users users)
    // {
    //     if (id != users.Id)
    //         return BadRequest();
    //
    //     context.Entry(users).State = EntityState.Modified;
    //
    //     try
    //     {
    //         await context.SaveChangesAsync();
    //     }
    //     catch (DbUpdateConcurrencyException)
    //     {
    //         if (!context.Users.Any(e => e.Id == id))
    //             return NotFound();
    //         throw;
    //     }
    //
    //     return NoContent();
    // }
    //
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteUser(int id)
    // {
    //     var user = await context.Users.FindAsync(id);
    //     if (user == null)
    //         return NotFound();
    //
    //     context.Users.Remove(user);
    //     await context.SaveChangesAsync();
    //
    //     return NoContent();
    // }
}