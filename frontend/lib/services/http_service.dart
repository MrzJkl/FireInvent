import 'dart:convert';
import 'package:http/http.dart' as http;

class HttpService {
  final String _baseUrl = "http://localhost:5004";

  void setToken(String token) {
    _token = token;
  }

  Map<String, String> _headers() => {
    'Content-Type': 'application/json',
    if (_token != null) 'Authorization': 'Bearer $_token',
  };

  Future<http.Response> get(String path) async {
    final uri = Uri.parse('$_baseUrl$path');
    final response = await http.get(uri, headers: _headers());
    _handleErrors(response);
    return response;
  }

  Future<http.Response> post(String path, dynamic body) async {
    final uri = Uri.parse('$_baseUrl$path');
    final response = await http.post(uri, headers: _headers(), body: jsonEncode(body));
    _handleErrors(response);
    return response;
  }

  Future<http.Response> put(String path, dynamic body) async {
    final uri = Uri.parse('$_baseUrl$path');
    final response = await http.put(uri, headers: _headers(), body: jsonEncode(body));
    _handleErrors(response);
    return response;
  }

  Future<http.Response> delete(String path) async {
    final uri = Uri.parse('$_baseUrl$path');
    final response = await http.delete(uri, headers: _headers());
    _handleErrors(response);
    return response;
  }

  void _handleErrors(http.Response response) {
    if (response.statusCode >= 400) {
      throw HttpException(
        response.statusCode,
        response.body,
      );
    }
  }
}

class HttpException implements Exception {
  final int statusCode;
  final String body;

  HttpException(this.statusCode, this.body);

  @override
  String toString() => 'HttpException: $statusCode - $body';
}
