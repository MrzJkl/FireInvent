import 'dart:convert';
import 'package:get_it/get_it.dart';

import '../models/person_model.dart';
import '../services/http_service.dart';

class PersonService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<PersonModel>> getAll() async {
    final response = await http.get('/persons');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => PersonModel.fromJson(e)).toList();
  }

  Future<PersonModel> getById(String id) async {
    final response = await http.get('/persons/$id');
    return PersonModel.fromJson(jsonDecode(response.body));
  }

  Future<PersonModel> create(PersonModel model) async {
    final response = await http.post('/persons', model.toJson());
    return PersonModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, PersonModel model) async {
    await http.put('/persons/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/persons/$id');
  }
}
