import 'dart:convert';
import 'package:get_it/get_it.dart';

import '../models/login_model.dart';
import 'package:flameguardlaundry/services/http_service.dart';

class AuthService {
  final HttpService http = GetIt.I<HttpService>();

  Future<void> login(LoginModel model) async {
    final response = await http.post('/auth/login', model.toJson());
    final Map<String, dynamic> json = jsonDecode(response.body);

    if (json.containsKey('token')) {
      http.setToken(json['token']);
    }
  }
}