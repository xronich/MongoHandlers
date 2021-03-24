using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace .Domain.Implementation.Schemas
{
    public static class FluentValidationJsonSchemaExtension
    {
        public static IRuleBuilderInitial<T, JToken> Schema<T>(this IRuleBuilder<T, JToken> ruleBuilder,
            Func<JSchema> schemaFactory)
        {
            var ruleBuilderInitial = ruleBuilder.Custom((obj, context) =>
            {
                var jSchema = schemaFactory();

                var isValid = obj.IsValid(jSchema, out IList<ValidationError> errors);

                if (isValid)
                {
                    return;
                }

                foreach (var validationError in errors)
                {
                    context.AddFailure(new ValidationFailure(validationError.Path, validationError.Message)
                    {
                        CustomState = validationError
                    });
                }
            });

            return ruleBuilderInitial;
        }
    }
}