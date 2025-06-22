import 'dart:convert';
import 'package:get_it/get_it.dart';

import '../models/user_model.dart';
import '../models/create_user_model.dart';
import '../services/http_service.dart';

class UserService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<UserModel>> getAll() async {
    final response = await http.get('/users');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => UserModel.fromJson(e)).toList();
  }

  Future<void> create(CreateUserModel model) async {
    await http.post('/users', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/users/$id');
  }

  Future<String> resetPassword(String id) async {
    final response = await http.post('/users/$id/reset-password', {});
    return response.body;
  }
}
