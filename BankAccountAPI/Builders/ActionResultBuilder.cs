using Microsoft.AspNetCore.Mvc;
using System;

namespace BankAccountAPI.Builders
{
    public class ActionResultBuilder<T>
    {
        private T? _data;
        private int _statusCode = 200;
        private string? _errorMessage;
        private bool _isError = false;

        public static ActionResultBuilder<T> Create()
        {
            return new ActionResultBuilder<T>();
        }

        public ActionResultBuilder<T> WithData(T data)
        {
            _data = data;
            return this;
        }

        public ActionResultBuilder<T> WithStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public ActionResultBuilder<T> WithError(string errorMessage)
        {
            _errorMessage = errorMessage;
            _isError = true;
            return this;
        }

        public ActionResultBuilder<T> AsOk()
        {
            _statusCode = 200;
            return this;
        }

        public ActionResultBuilder<T> AsCreated()
        {
            _statusCode = 201;
            return this;
        }

        public ActionResultBuilder<T> AsNoContent()
        {
            _statusCode = 204;
            return this;
        }

        public ActionResultBuilder<T> AsNotFound()
        {
            _statusCode = 404;
            _isError = true;
            return this;
        }

        public ActionResultBuilder<T> AsBadRequest()
        {
            _statusCode = 400;
            _isError = true;
            return this;
        }

        public ActionResult<T> Build()
        {
            if (_isError)
            {
                return BuildError();
            }

            return _statusCode switch
            {
                200 => new OkObjectResult(_data),
                201 => new CreatedResult((string?)null, _data),
                204 => new NoContentResult(),
                _ => new ObjectResult(_data) { StatusCode = _statusCode }
            };
        }

        public CreatedAtActionResult BuildCreatedAtAction(string actionName, object? routeValues, T value)
        {
            return new CreatedAtActionResult(actionName, null, routeValues, value);
        }

        private ActionResult BuildError()
        {
            return _statusCode switch
            {
                404 => new NotFoundResult(),
                400 => new BadRequestResult(),
                _ => new ObjectResult(_errorMessage ?? "An error occurred") { StatusCode = _statusCode }
            };
        }
    }

    public class ActionResultBuilder
    {
        private int _statusCode = 200;
        private string? _errorMessage;
        private bool _isError = false;

        public static ActionResultBuilder Create()
        {
            return new ActionResultBuilder();
        }

        public ActionResultBuilder WithStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public ActionResultBuilder WithError(string errorMessage)
        {
            _errorMessage = errorMessage;
            _isError = true;
            return this;
        }

        public ActionResultBuilder AsNoContent()
        {
            _statusCode = 204;
            return this;
        }

        public ActionResultBuilder AsNotFound()
        {
            _statusCode = 404;
            _isError = true;
            return this;
        }

        public ActionResultBuilder AsBadRequest()
        {
            _statusCode = 400;
            _isError = true;
            return this;
        }

        public ActionResult Build()
        {
            if (_isError)
            {
                return BuildError();
            }

            return _statusCode switch
            {
                204 => new NoContentResult(),
                _ => new StatusCodeResult(_statusCode)
            };
        }

        private ActionResult BuildError()
        {
            return _statusCode switch
            {
                404 => new NotFoundResult(),
                400 => new BadRequestResult(),
                _ => new ObjectResult(_errorMessage ?? "An error occurred") { StatusCode = _statusCode }
            };
        }
    }
}
