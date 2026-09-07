using System;
using System.Collections.Generic;
using Adletec.Sonic;
using UnityEngine;

namespace PolarGeometry
{
    public class StringPolarFunction : PolarFunction
    {
        [Serializable]
        public class Parameter
        {
            [SerializeField]
            private string name;

            [SerializeField]
            private float value;

            public string Name
            {
                get => name;
                set => name = value;
            }

            public float Value
            {
                get => value;
                set => this.value = value;
            }

            public Parameter(
                string name,
                float value)
            {
                this.name = name;
                this.value = value;
            }
        }

        private const string ThetaVariableName =
            "theta";

        [Header("Expression")]
        [SerializeField]
        private string expression =
            "cos(theta)";

        [Header("Parameters")]
        [SerializeField]
        private List<Parameter> parameters =
            new List<Parameter>();

        [Header("Range")]
        [SerializeField]
        [Min(0f)]
        private float thetaSpanDegrees = 0f;

        private Evaluator evaluator;

        private Func<
            Dictionary<string, double>,
            double
        > evaluateDelegate;

        private Dictionary<string, double>
            appliedVariables =
                new Dictionary<string, double>();

        private List<string>
            appliedVariableNames =
                new List<string>();

        private string appliedExpression;

        private float appliedThetaSpanDegrees;

        private string errorMessage;

        public string Expression
        {
            get => expression;
            set => expression = value;
        }

        public IReadOnlyList<Parameter> Parameters =>
            parameters;

        public float ThetaSpanDegrees
        {
            get => thetaSpanDegrees;
            set => thetaSpanDegrees = value;
        }

        public string AppliedExpression =>
            appliedExpression;

        public string ErrorMessage =>
            errorMessage;

        public bool IsValid =>
            evaluateDelegate != null;

        public override string Formula =>
            appliedExpression;

        public override float? ThetaSpan =>
            appliedThetaSpanDegrees > 0f
                ? appliedThetaSpanDegrees *
                  Mathf.Deg2Rad
                : null;

        private void Awake()
        {
            evaluator =
                Evaluator.CreateWithDefaults();

            TryApply(
                notifyChanged: false
            );
        }

        public override float Evaluate(
            float theta)
        {
            if (evaluateDelegate == null)
            {
                return 0f;
            }

            appliedVariables[
                ThetaVariableName
            ] = theta;

            double result =
                evaluateDelegate(
                    appliedVariables
                );

            return (float)result;
        }

        public bool Apply()
        {
            return TryApply(
                notifyChanged: true
            );
        }

        public bool SetParameterValue(
            string name,
            float value)
        {
            for (
                int i = 0;
                i < parameters.Count;
                i++)
            {
                if (parameters[i].Name != name)
                    continue;

                parameters[i].Value = value;

                return true;
            }

            return false;
        }

        public bool TryGetParameterValue(
            string name,
            out float value)
        {
            for (
                int i = 0;
                i < parameters.Count;
                i++)
            {
                if (parameters[i].Name != name)
                    continue;

                value =
                    parameters[i].Value;

                return true;
            }

            value = default;

            return false;
        }

        public void AddParameter(
            string name,
            float value = 0f)
        {
            parameters.Add(
                new Parameter(
                    name,
                    value
                )
            );
        }

        public bool RemoveParameter(
            string name)
        {
            for (
                int i = 0;
                i < parameters.Count;
                i++)
            {
                if (parameters[i].Name != name)
                    continue;

                parameters.RemoveAt(i);

                return true;
            }

            return false;
        }

        private bool TryApply(
            bool notifyChanged)
        {
            if (string.IsNullOrWhiteSpace(
                expression))
            {
                return Fail(
                    "Expression is empty."
                );
            }

            if (thetaSpanDegrees < 0f)
            {
                return Fail(
                    "Theta Span must be zero or greater."
                );
            }

            if (!TryBuildVariables(
                out Dictionary<string, double>
                    newVariables,
                out List<string>
                    newVariableNames))
            {
                return false;
            }

            bool requiresCompile =
                RequiresCompile(
                    newVariableNames
                );

            Func<
                Dictionary<string, double>,
                double
            > newDelegate =
                evaluateDelegate;

            if (requiresCompile)
            {
                try
                {
                    evaluator.Validate(
                        expression,
                        newVariableNames
                    );

                    newDelegate =
                        evaluator.CreateDelegate(
                            expression
                        );
                }
                catch (Exception exception)
                {
                    return Fail(
                        exception.Message
                    );
                }
            }

            bool changed =
                HasAppliedStateChanged(
                    newVariables,
                    newVariableNames
                );

            // ここまで成功してから、
            // runtime状態をまとめて更新する。
            appliedVariables =
                newVariables;

            appliedThetaSpanDegrees =
                thetaSpanDegrees;

            if (requiresCompile)
            {
                evaluateDelegate =
                    newDelegate;

                appliedExpression =
                    expression;

                appliedVariableNames =
                    new List<string>(
                        newVariableNames
                    );
            }

            errorMessage = null;

            if (
                notifyChanged &&
                changed
            )
            {
                NotifyChanged();
            }

            return true;
        }

        private bool TryBuildVariables(
            out Dictionary<string, double>
                variables,
            out List<string>
                variableNames)
        {
            variables =
                new Dictionary<string, double>();

            variableNames =
                new List<string>();

            variables.Add(
                ThetaVariableName,
                0d
            );

            variableNames.Add(
                ThetaVariableName
            );

            for (
                int i = 0;
                i < parameters.Count;
                i++)
            {
                Parameter parameter =
                    parameters[i];

                string parameterName =
                    parameter.Name;

                if (string.IsNullOrWhiteSpace(
                    parameterName))
                {
                    Fail(
                        $"Parameter {i} has no name."
                    );

                    return false;
                }

                if (
                    parameterName ==
                    ThetaVariableName
                )
                {
                    Fail(
                        "\"theta\" is reserved."
                    );

                    return false;
                }

                if (
                    parameterName == "pi" ||
                    parameterName == "e"
                )
                {
                    Fail(
                        $"\"{parameterName}\" is reserved."
                    );

                    return false;
                }

                if (variables.ContainsKey(
                    parameterName))
                {
                    Fail(
                        $"Parameter \"{parameterName}\" is duplicated."
                    );

                    return false;
                }

                variables.Add(
                    parameterName,
                    parameter.Value
                );

                variableNames.Add(
                    parameterName
                );
            }

            return true;
        }

        private bool RequiresCompile(
            List<string> variableNames)
        {
            if (evaluateDelegate == null)
                return true;

            if (appliedExpression != expression)
                return true;

            if (
                appliedVariableNames.Count !=
                variableNames.Count
            )
            {
                return true;
            }

            for (
                int i = 0;
                i < variableNames.Count;
                i++)
            {
                if (
                    appliedVariableNames[i] !=
                    variableNames[i]
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasAppliedStateChanged(
            Dictionary<string, double>
                newVariables,
            List<string>
                newVariableNames)
        {
            if (
                appliedExpression !=
                expression
            )
            {
                return true;
            }

            if (
                appliedThetaSpanDegrees !=
                thetaSpanDegrees
            )
            {
                return true;
            }

            if (
                appliedVariableNames.Count !=
                newVariableNames.Count
            )
            {
                return true;
            }

            for (
                int i = 0;
                i < newVariableNames.Count;
                i++)
            {
                string variableName =
                    newVariableNames[i];

                if (
                    !appliedVariables.TryGetValue(
                        variableName,
                        out double oldValue
                    )
                )
                {
                    return true;
                }

                if (
                    oldValue !=
                    newVariables[variableName]
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool Fail(
            string message)
        {
            errorMessage = message;

            Debug.LogWarning(
                $"Failed to apply polar function: {message}",
                this
            );

            return false;
        }
    }
}
