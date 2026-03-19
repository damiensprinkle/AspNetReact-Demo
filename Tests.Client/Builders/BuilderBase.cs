using System.Linq.Expressions;
using System.Reflection;

namespace Tests.Client.Builders
{
    /// <summary>
    /// Base class for fluent DTO builders.
    /// Mutations are stored and applied to a fresh default instance on each <see cref="Build"/> call,
    /// so the same builder can be reused and individual builds remain independent.
    /// </summary>
    public abstract class BuilderBase<TDto, TBuilder>
        where TDto : new()
        where TBuilder : BuilderBase<TDto, TBuilder>
    {
        private readonly List<Action<TDto>> _mutations = new();

        /// <summary>
        /// Returns a new instance populated with sensible defaults.
        /// Override in each concrete builder to supply domain-appropriate values.
        /// </summary>
        protected abstract TDto Defaults();

        /// <summary>
        /// Queues a mutation that sets the property identified by <paramref name="property"/>
        /// to <paramref name="value"/> when <see cref="Build"/> is called.
        /// </summary>
        public TBuilder Set<TValue>(Expression<Func<TDto, TValue>> property, TValue value)
        {
            var propInfo = (PropertyInfo)((MemberExpression)property.Body).Member;
            _mutations.Add(dto => propInfo.SetValue(dto, value));
            return (TBuilder)this;
        }

        /// <summary>
        /// Creates a new <typeparamref name="TDto"/> from defaults, applies all queued mutations,
        /// and returns the result.
        /// </summary>
        public TDto Build()
        {
            var dto = Defaults();
            foreach (var mutation in _mutations)
                mutation(dto);
            return dto;
        }
    }
}
