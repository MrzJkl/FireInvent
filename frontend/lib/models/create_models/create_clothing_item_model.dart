import 'package:flameguardlaundry/models/gear_condition.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_clothing_item_model.g.dart';

@JsonSerializable()
@immutable
class CreateClothingItemModel {
  final String variantId;
  final String? identifier;
  final String? storageLocationId;
  final GearCondition condition;
  final DateTime purchaseDate;
  final DateTime? retirementDate;

  const CreateClothingItemModel({
    required this.variantId,
    this.identifier,
    this.storageLocationId,
    required this.condition,
    required this.purchaseDate,
    required this.retirementDate,
  });

  factory CreateClothingItemModel.fromJson(Map<String, dynamic> json) =>
      _$CreateClothingItemModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateClothingItemModelToJson(this);
}
