import 'package:flameguardlaundry/constants.dart';
import 'package:flameguardlaundry/main_drawer.dart';
import 'package:flameguardlaundry/models/create_models/create_clothing_product_model.dart';
import 'package:flameguardlaundry/models/create_models/create_person_model.dart';
import 'package:flameguardlaundry/models/gear_type.dart';
import 'package:flameguardlaundry/models/person_model.dart';
import 'package:flameguardlaundry/services/person_service.dart';
import 'package:flameguardlaundry/widgets/person_form.dart';
import 'package:flameguardlaundry/widgets/persons_table.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import '../models/clothing_product_model.dart';

class PersonListScreen extends StatefulWidget {
  const PersonListScreen({super.key});

  @override
  State<PersonListScreen> createState() => _PersonListScreenState();
}

class _PersonListScreenState extends State<PersonListScreen> {
  late Future<List<PersonModel>> _personsFuture;
  final PersonService _service = GetIt.I<PersonService>();

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  void _loadData() {
    setState(() {
      _personsFuture = _service.getAll();
    });
  }

  void _delete(String id) async {
    await _service.delete(id);
    _loadData();
  }

  Future<void> showPersonFormDialog({
    required BuildContext context,
    PersonModel? initial,
    required void Function(String? id, CreatePersonModel) onSubmit,
  }) async {
    await showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: Text(initial == null ? 'Create Person' : 'Edit Person'),
            content: SizedBox(
              width: 400,
              child: PersonForm(
                initial: initial,
                onSubmit: (createModel) {
                  Navigator.of(context).pop();
                  onSubmit(initial?.id, createModel);
                },
              ),
            ),
          ),
    );
  }

  void _edit(PersonModel model) async {
    await showPersonFormDialog(
      context: context,
      initial: model,
      onSubmit: (id, updated) async {
        final updatedModel = PersonModel(
          id: id!,
          firstName: updated.firstName,
          lastName: updated.lastName,
          contactInfo: updated.contactInfo,
          externalId: updated.externalId,
          remarks: updated.remarks,
        );
        await _service.update(id, updatedModel);
        _loadData();
      },
    );
  }

  void _create() async {
    await showPersonFormDialog(
      context: context,
      onSubmit: (id, created) async {
        await _service.create(created);
        _loadData();
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: Constants.buildMainAppBar(context),
      drawer: MainDrawer(),
      floatingActionButton: FloatingActionButton(
        onPressed: _create,
        tooltip: 'Add Person',
        child: const Icon(Icons.add),
      ),
      body: FutureBuilder<List<PersonModel>>(
        future: _personsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No persons available.'));
          }

          final persons = snapshot.data!;

          return PersonsTable(
            persons: persons,
            onEdit: _edit,
            onDelete: _delete,
          );
        },
      ),
    );
  }
}
