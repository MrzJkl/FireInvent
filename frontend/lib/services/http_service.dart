import 'dart:convert';
import 'package:http/http.dart' as http;

class HttpService {
  final String _baseUrl = "http://localhost:5004";
  String? _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidXNlcjFAZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI4MDQ0NmU1LTI3OGMtNDczNy1hNmUyLTg3ZDVhNGE2ODBkOCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6WyJ1c2VyIiwiYWRtaW4iXSwiZXhwIjoxNzUwMTU4MjUwLCJpc3MiOiJmbGFtZWd1YXJkbGF1bmRyeSIsImF1ZCI6ImZsYW1lZ3VhcmRsYXVuZHJ5LXVzZXJzIn0.m3ue19fK62TF8Jx2grso4X3fQnlWSnDuNI3CWj0FL_Y";


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
