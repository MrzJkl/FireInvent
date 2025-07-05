import 'dart:convert';
import 'package:flameguardlaundry/models/create_models/create_clothing_item_model.dart';
import 'package:get_it/get_it.dart';

import '../models/clothing_item_model.dart';
import '../services/http_service.dart';

class ClothingItemService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<ClothingItemModel>> getAll() async {
    final response = await http.get('/clothingItems');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => ClothingItemModel.fromJson(e)).toList();
  }

  Future<ClothingItemModel> getById(String id) async {
    final response = await http.get('/clothingItems/$id');
    return ClothingItemModel.fromJson(jsonDecode(response.body));
  }

  Future<ClothingItemModel> create(CreateClothingItemModel model) async {
    final response = await http.post('/clothingItems', model.toJson());
    return ClothingItemModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, ClothingItemModel model) async {
    await http.put('/clothingItems/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/clothingItems/$id');
  }
}
