using Microsoft.EntityFrameworkCore;

namespace AdventureWorks
{
   public static class Extensions
   {
      public static void SetState<T>(this AdventureWorksContext context, T entity, string _state) where T : class
      {
         EntityState state;
         switch (_state) {
            case "Added":
               state = EntityState.Added;
               break;
            case "Modified":
               state = EntityState.Modified;
               break;
            case "Deleted":
               state = EntityState.Deleted;
               break;
            default:
               state = EntityState.Unchanged;
               break;
         }
         context.Entry(entity).State = state;
      }
   }
}