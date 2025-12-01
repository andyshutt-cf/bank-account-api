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
                return BuildErrorResult();
            }

            return _statusCode switch
            {
                200 => new OkObjectResult(_data),
                201 => new CreatedResult(string.Empty, _data),
                204 => new NoContentResult(),
                _ => new ObjectResult(_data) { StatusCode = _statusCode }
            };
        }

        public ActionResult BuildWithoutValue()
        {
            if (_isError)
            {
                return BuildErrorResultWithoutValue();
            }

            return _statusCode switch
            {
                204 => new NoContentResult(),
                400 => new BadRequestResult(),
                404 => new NotFoundResult(),
                _ => new StatusCodeResult(_statusCode)
            };
        }

        public CreatedAtActionResult BuildCreatedAtAction(string actionName, object? routeValues, T value)
        {
            return new CreatedAtActionResult(actionName, null, routeValues, value);
        }

        private ActionResult<T> BuildErrorResult()
        {
            return _statusCode switch
            {
                404 => new NotFoundResult(),
                400 => new BadRequestResult(),
                _ => new ObjectResult(_errorMessage ?? "An error occurred") { StatusCode = _statusCode }
            };
        }

        private ActionResult BuildErrorResultWithoutValue()
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
