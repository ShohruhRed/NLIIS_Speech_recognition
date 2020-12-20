using System.Collections.Generic;

namespace NLIIS_Speech_recognition.Services
{
    public class Actor
    {
        private readonly IEnumerable<Action> _actions = new List<Action>
        {
            new CreateNoteAction(),
            new PlayAuthorCompositionsAction(),
            new CreateEssayAction()
        };

        public string Act(string stringAction, string language)
        {
            var action = Action.GetApplicableAction(_actions, stringAction.ToLower(), language);

            return action == null ? "Unable to go with proposed action" : action.Run(stringAction, language);
        }
    }
}
