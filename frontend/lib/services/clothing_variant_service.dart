import 'dart:convert';
import 'package:flameguardlaundry/models/create_models/create_clothing_variant_model.dart';
import 'package:get_it/get_it.dart';

import '../models/clothing_variant_model.dart';
import '../models/clothing_item_model.dart';
import '../services/http_service.dart';

class ClothingVariantService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<ClothingVariantModel>> getAll() async {
    final response = await http.get('/clothingVariants');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => ClothingVariantModel.fromJson(e)).toList();
  }

  Future<ClothingVariantModel> getById(String id) async {
    final response = await http.get('/clothingVariants/$id');
    return ClothingVariantModel.fromJson(jsonDecode(response.body));
  }

  Future<ClothingVariantModel> create(CreateClothingVariantModel model) async {
    final response = await http.post('/clothingVariants', model.toJson());
    return ClothingVariantModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, ClothingVariantModel model) async {
    await http.put('/clothingVariants/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/clothingVariants/$id');
  }

  Future<List<ClothingItemModel>> getItems(String id) async {
    final response = await http.get('/clothingVariants/$id/items');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => ClothingItemModel.fromJson(e)).toList();
  }
}
